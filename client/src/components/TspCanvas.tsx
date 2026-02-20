import { useRef, useEffect } from 'react';
import type { City, IterationResult } from '../types';

interface Props {
  cities: City[];
  currentFrame: IterationResult | null;
  isComplete?: boolean; // true when showing the final optimized route
  width?: number;
  height?: number;
}

export function TspCanvas({ cities, currentFrame, isComplete = false, width = 600, height = 500 }: Props) {
  const canvasRef = useRef<HTMLCanvasElement>(null);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    ctx.clearRect(0, 0, width, height);

    if (cities.length === 0) {
      ctx.fillStyle = '#9ca3af';
      ctx.font = '14px sans-serif';
      ctx.textAlign = 'center';
      ctx.fillText('Generate or select a problem to begin', width / 2, height / 2);
      ctx.textAlign = 'left';
      return;
    }

    // ── Uniform aspect-ratio scaling (preserves circle shape) ──────────────
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

    // ── Layer 1: Naive sequential tour (always shown as grey underlay) ──────
    const sequentialRoute = cities.map((_, i) => i);
    drawRoute(sequentialRoute, '#e5e7eb', 1.5, true);

    // ── Layer 2: Current best route ─────────────────────────────────────────
    if (currentFrame && currentFrame.bestRoute.length > 0) {
      // Green when complete (final solution), blue while searching
      const routeColor = isComplete ? '#22c55e' : '#3b82f6';
      const routeWidth = isComplete ? 2.5 : 2;
      drawRoute(currentFrame.bestRoute, routeColor, routeWidth);
    }

    // ── Direction arrow on the first edge ───────────────────────────────────
    const activeRoute = currentFrame?.bestRoute ?? sequentialRoute;
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
      ctx.strokeStyle = isComplete ? '#16a34a' : currentFrame ? '#2563eb' : '#9ca3af';
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

    // ── "Complete" badge ────────────────────────────────────────────────────
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
    }
  }, [cities, currentFrame, isComplete, width, height]);

  return (
    <canvas
      ref={canvasRef}
      width={width}
      height={height}
      className="border border-gray-300 rounded-lg bg-white"
    />
  );
}
