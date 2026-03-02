import { useRef, useEffect, useState, useCallback, useMemo } from 'react';
import type { AlgorithmType, City, IterationResult } from '../types';

interface Props {
  cities: City[];
  currentFrame: IterationResult | null;
  initialRoute?: number[]; // randomized underlay; falls back to sequential if omitted
  isComplete?: boolean;    // true when showing the final optimized route
  isRunning?: boolean;     // true while optimization is in progress
  algorithmType?: AlgorithmType;
  width?: number;
  height?: number;
}

export function TspCanvas({
  cities,
  currentFrame,
  initialRoute,
  isComplete = false,
  isRunning = false,
  algorithmType,
  width: maxWidth = 700,
  height: maxHeight = 500,
}: Props) {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const containerRef = useRef<HTMLDivElement>(null);
  const [containerWidth, setContainerWidth] = useState(maxWidth);

  const onResize = useCallback((entries: ResizeObserverEntry[]) => {
    const w = entries[0]?.contentRect.width;
    if (w) setContainerWidth(w);
  }, []);

  useEffect(() => {
    const el = containerRef.current;
    if (!el) return;
    const observer = new ResizeObserver(onResize);
    observer.observe(el);
    return () => observer.disconnect();
  }, [onResize]);

  const width = Math.min(containerWidth, maxWidth);
  const height = Math.round(width * (maxHeight / maxWidth));

  const transform = useMemo(() => {
    if (cities.length === 0) return null;

    const padding = 48;
    const xs = cities.map((c) => c.x);
    const ys = cities.map((c) => c.y);
    const minX = Math.min(...xs), maxX = Math.max(...xs);
    const minY = Math.min(...ys), maxY = Math.max(...ys);
    const rangeX = maxX - minX || 1;
    const rangeY = maxY - minY || 1;

    const scaleUniform = Math.min(
      (width - 2 * padding) / rangeX,
      (height - 2 * padding) / rangeY,
    );
    const drawW = rangeX * scaleUniform;
    const drawH = rangeY * scaleUniform;
    const originX = padding + (width - 2 * padding - drawW) / 2;
    const originY = padding + (height - 2 * padding - drawH) / 2;

    const toCanvas = (city: City) => ({
      sx: originX + (city.x - minX) * scaleUniform,
      sy: originY + (city.y - minY) * scaleUniform,
    });

    return { toCanvas, originX, originY, scaleUniform };
  }, [cities, width, height]);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    ctx.clearRect(0, 0, width, height);

    if (cities.length === 0 || !transform) {
      ctx.fillStyle = '#9ca3af';
      ctx.font = '14px sans-serif';
      ctx.textAlign = 'center';
      ctx.fillText('Generate or select a problem to begin', width / 2, height / 2);
      ctx.textAlign = 'left';
      return;
    }

    const { toCanvas } = transform;

    // ── Route drawing helper ────────────────────────────────────────────────
    const drawRoute = (route: number[], color: string, lineWidth: number, dashed = false) => {
      if (route.length < 2) return;
      ctx.beginPath();
      ctx.strokeStyle = color;
      ctx.lineWidth = lineWidth;
      ctx.setLineDash(dashed ? [6, 4] : []);
      const first = toCanvas(cities[route[0]]);
      ctx.moveTo(first.sx, first.sy);
      for (let i = 1; i < route.length; i++) {
        const p = toCanvas(cities[route[i]]);
        ctx.lineTo(p.sx, p.sy);
      }
      ctx.lineTo(first.sx, first.sy); // close tour
      ctx.stroke();
      ctx.setLineDash([]);
    };

    // ── Layer 1: Initial random tour (grey dashed underlay) ─────────────────
    const underlayRoute =
      initialRoute && initialRoute.length === cities.length
        ? initialRoute
        : cities.map((_, i) => i); // fallback to sequential
    drawRoute(underlayRoute, '#e5e7eb', 1.5, true);

    // ── Layer 2: Pheromone trails (ACO) ────────────────────────────────────
    if (algorithmType === 'AntColonyOptimization' && currentFrame?.metadata?.pheromoneEdges) {
      const edges = currentFrame.metadata.pheromoneEdges as number[][];
      for (const [from, to, strength] of edges) {
        const fromCity = toCanvas(cities[from]);
        const toCity = toCanvas(cities[to]);
        ctx.beginPath();
        ctx.strokeStyle = `rgba(245, 158, 11, ${0.15 + strength * 0.6})`;
        ctx.lineWidth = 0.5 + strength * 3;
        ctx.moveTo(fromCity.sx, fromCity.sy);
        ctx.lineTo(toCity.sx, toCity.sy);
        ctx.stroke();
      }
    }

    // ── Layer 3: Ghost routes (population-based algorithms) ──────────────
    const ghostRoutes = (currentFrame?.metadata?.sampleRoutes ?? currentFrame?.metadata?.particleRoutes) as number[][] | undefined;
    if (ghostRoutes) {
      for (const route of ghostRoutes) {
        if (route.length < 2) continue;
        ctx.beginPath();
        ctx.strokeStyle = 'rgba(167, 139, 250, 0.25)';
        ctx.lineWidth = 1;
        const first = toCanvas(cities[route[0]]);
        ctx.moveTo(first.sx, first.sy);
        for (let i = 1; i < route.length; i++) {
          const p = toCanvas(cities[route[i]]);
          ctx.lineTo(p.sx, p.sy);
        }
        ctx.lineTo(first.sx, first.sy);
        ctx.stroke();
      }
    }

    // ── Layer 4: Exploration/current route ───────────────────────────────
    if (currentFrame?.currentRoute && currentFrame.currentRoute.length >= 2) {
      ctx.beginPath();
      ctx.strokeStyle = 'rgba(239, 68, 68, 0.4)';
      ctx.lineWidth = 1.5;
      const first = toCanvas(cities[currentFrame.currentRoute[0]]);
      ctx.moveTo(first.sx, first.sy);
      for (let i = 1; i < currentFrame.currentRoute.length; i++) {
        const p = toCanvas(cities[currentFrame.currentRoute[i]]);
        ctx.lineTo(p.sx, p.sy);
      }
      ctx.lineTo(first.sx, first.sy);
      ctx.stroke();
    }

    // ── Layer 5: Current best route ──────────────────────────────────────
    if (currentFrame && currentFrame.bestRoute.length > 0) {
      // Green when complete (final solution), orange while running live, blue while replaying
      const routeColor = isComplete ? '#22c55e' : isRunning ? '#f97316' : '#3b82f6';
      const routeWidth = isComplete ? 2.5 : 2;
      drawRoute(currentFrame.bestRoute, routeColor, routeWidth);
    }

    // ── Direction arrow on the first edge ───────────────────────────────────
    const activeRoute = currentFrame?.bestRoute ?? underlayRoute;
    if (activeRoute.length >= 2) {
      const a = toCanvas(cities[activeRoute[0]]);
      const b = toCanvas(cities[activeRoute[1]]);
      const angle = Math.atan2(b.sy - a.sy, b.sx - a.sx);
      const mx = (a.sx + b.sx) / 2;
      const my = (a.sy + b.sy) / 2;
      const arrowLen = 8;
      ctx.save();
      ctx.translate(mx, my);
      ctx.rotate(angle);
      ctx.beginPath();
      ctx.moveTo(-arrowLen, -arrowLen / 2);
      ctx.lineTo(0, 0);
      ctx.lineTo(-arrowLen, arrowLen / 2);
      ctx.strokeStyle = isComplete ? '#16a34a' : isRunning ? '#ea580c' : currentFrame ? '#2563eb' : '#9ca3af';
      ctx.lineWidth = 1.5;
      ctx.stroke();
      ctx.restore();
    }

    // ── Cities ───────────────────────────────────────────────────────────────
    cities.forEach((city, i) => {
      const { sx, sy } = toCanvas(city);
      const isStart = i === 0;

      ctx.beginPath();
      ctx.arc(sx, sy, isStart ? 7 : 5, 0, Math.PI * 2);
      ctx.fillStyle = isStart ? '#f59e0b' : '#ef4444'; // gold for city 0
      ctx.fill();
      ctx.strokeStyle = '#fff';
      ctx.lineWidth = 1.5;
      ctx.stroke();

      ctx.fillStyle = '#1f2937';
      ctx.font = `${isStart ? 'bold ' : ''}10px sans-serif`;
      ctx.fillText(String(i), sx + 8, sy - 6);
    });

    // ── Status badge ────────────────────────────────────────────────────────
    if (isComplete) {
      const text = 'Optimized';
      ctx.font = 'bold 12px sans-serif';
      const tw = ctx.measureText(text).width;
      const bx = width - tw - 20, by = 12;
      ctx.fillStyle = '#dcfce7';
      ctx.strokeStyle = '#22c55e';
      ctx.lineWidth = 1;
      ctx.beginPath();
      ctx.roundRect(bx - 6, by - 1, tw + 12, 20, 4);
      ctx.fill();
      ctx.stroke();
      ctx.fillStyle = '#15803d';
      ctx.fillText(text, bx, by + 13);
    } else if (isRunning) {
      const text = 'Running...';
      ctx.font = 'bold 12px sans-serif';
      const tw = ctx.measureText(text).width;
      const bx = width - tw - 20, by = 12;
      ctx.fillStyle = '#fff7ed';
      ctx.strokeStyle = '#f97316';
      ctx.lineWidth = 1;
      ctx.beginPath();
      ctx.roundRect(bx - 6, by - 1, tw + 12, 20, 4);
      ctx.fill();
      ctx.stroke();
      ctx.fillStyle = '#c2410c';
      ctx.fillText(text, bx, by + 13);
    }

    // ── Temperature indicator (SA only) ──────────────────────────────────
    if (algorithmType === 'SimulatedAnnealing' && currentFrame?.metadata?.temperature != null) {
      const temp = currentFrame.metadata.temperature as number;
      const maxTemp = 10000;
      const barHeight = 60;
      const barWidth = 8;
      const barX = 12;
      const barY = 12;
      const fill = Math.min(1, Math.max(0, Math.log(temp + 1) / Math.log(maxTemp + 1)));

      ctx.fillStyle = '#e5e7eb';
      ctx.fillRect(barX, barY, barWidth, barHeight);

      const fillHeight = fill * barHeight;
      const gradient = ctx.createLinearGradient(barX, barY + barHeight, barX, barY);
      gradient.addColorStop(0, '#3b82f6');
      gradient.addColorStop(1, '#ef4444');
      ctx.fillStyle = gradient;
      ctx.fillRect(barX, barY + barHeight - fillHeight, barWidth, fillHeight);

      ctx.strokeStyle = '#9ca3af';
      ctx.lineWidth = 0.5;
      ctx.strokeRect(barX, barY, barWidth, barHeight);

      ctx.fillStyle = '#6b7280';
      ctx.font = '9px sans-serif';
      ctx.fillText('T', barX + 1, barY + barHeight + 10);
    }
  }, [cities, currentFrame, initialRoute, isComplete, isRunning, algorithmType, width, height, transform]);

  return (
    <div ref={containerRef} className="max-w-full">
      <canvas
        ref={canvasRef}
        width={width}
        height={height}
        role="img"
        aria-label="TSP route visualization"
        className="border border-gray-300 rounded-lg bg-white max-w-full"
      />
    </div>
  );
}
