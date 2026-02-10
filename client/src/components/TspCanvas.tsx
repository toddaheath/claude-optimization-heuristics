import { useRef, useEffect } from 'react';
import type { City, IterationResult } from '../types';

interface Props {
  cities: City[];
  currentFrame: IterationResult | null;
  width?: number;
  height?: number;
}

export function TspCanvas({ cities, currentFrame, width = 600, height = 500 }: Props) {
  const canvasRef = useRef<HTMLCanvasElement>(null);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas || cities.length === 0) return;

    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    const padding = 40;
    const xs = cities.map((c) => c.x);
    const ys = cities.map((c) => c.y);
    const minX = Math.min(...xs);
    const maxX = Math.max(...xs);
    const minY = Math.min(...ys);
    const maxY = Math.max(...ys);
    const rangeX = maxX - minX || 1;
    const rangeY = maxY - minY || 1;

    const scale = (city: City) => ({
      sx: padding + ((city.x - minX) / rangeX) * (width - 2 * padding),
      sy: padding + ((city.y - minY) / rangeY) * (height - 2 * padding),
    });

    ctx.clearRect(0, 0, width, height);

    // Draw route
    if (currentFrame && currentFrame.bestRoute.length > 0) {
      ctx.beginPath();
      ctx.strokeStyle = '#3b82f6';
      ctx.lineWidth = 2;
      const first = scale(cities[currentFrame.bestRoute[0]]);
      ctx.moveTo(first.sx, first.sy);
      for (let i = 1; i < currentFrame.bestRoute.length; i++) {
        const p = scale(cities[currentFrame.bestRoute[i]]);
        ctx.lineTo(p.sx, p.sy);
      }
      ctx.lineTo(first.sx, first.sy);
      ctx.stroke();
    }

    // Draw cities
    cities.forEach((city, i) => {
      const { sx, sy } = scale(city);
      ctx.beginPath();
      ctx.arc(sx, sy, 5, 0, Math.PI * 2);
      ctx.fillStyle = '#ef4444';
      ctx.fill();
      ctx.fillStyle = '#1f2937';
      ctx.font = '10px sans-serif';
      ctx.fillText(String(i), sx + 7, sy - 7);
    });
  }, [cities, currentFrame, width, height]);

  return (
    <canvas
      ref={canvasRef}
      width={width}
      height={height}
      className="border border-gray-300 rounded-lg bg-white"
    />
  );
}
