import type { City } from '../types';

export function generateRandomCities(count: number): City[] {
  return Array.from({ length: count }, (_, i) => ({
    id: i,
    x: Math.round(Math.random() * 460 + 20),
    y: Math.round(Math.random() * 360 + 20),
  }));
}

export function generateCircleCities(count: number): City[] {
  const cx = 250, cy = 200, radius = 180;
  return Array.from({ length: count }, (_, i) => ({
    id: i,
    x: Math.round(cx + radius * Math.cos((2 * Math.PI * i) / count)),
    y: Math.round(cy + radius * Math.sin((2 * Math.PI * i) / count)),
  }));
}

export function generateSquareCities(count: number): City[] {
  const margin = 40;
  const w = 420, h = 320;
  const perimeter = 2 * (w + h);
  return Array.from({ length: count }, (_, i) => {
    const d = (i / count) * perimeter;
    let x: number, y: number;
    if (d < w) {
      x = margin + d;               y = margin;
    } else if (d < w + h) {
      x = margin + w;               y = margin + (d - w);
    } else if (d < 2 * w + h) {
      x = margin + w - (d - w - h); y = margin + h;
    } else {
      x = margin;                   y = margin + h - (d - 2 * w - h);
    }
    return { id: i, x: Math.round(x), y: Math.round(y) };
  });
}

export function generateTriangleCities(count: number): City[] {
  const cx = 250, cy = 200, side = 340;
  const th = (Math.sqrt(3) / 2) * side;
  const vertices = [
    { x: cx,             y: cy - (th * 2) / 3 },
    { x: cx - side / 2,  y: cy + th / 3 },
    { x: cx + side / 2,  y: cy + th / 3 },
  ];
  const perimeter = side * 3;
  return Array.from({ length: count }, (_, i) => {
    const d = (i / count) * perimeter;
    const sideIdx = Math.min(Math.floor(d / side), 2);
    const t = (d - sideIdx * side) / side;
    const from = vertices[sideIdx];
    const to = vertices[(sideIdx + 1) % 3];
    return { id: i, x: Math.round(from.x + t * (to.x - from.x)), y: Math.round(from.y + t * (to.y - from.y)) };
  });
}

export function generatePentagonCities(count: number): City[] {
  const cx = 250, cy = 200, radius = 175;
  const sides = 5;
  const vertices = Array.from({ length: sides }, (_, k) => ({
    x: cx + radius * Math.cos((2 * Math.PI * k) / sides - Math.PI / 2),
    y: cy + radius * Math.sin((2 * Math.PI * k) / sides - Math.PI / 2),
  }));
  const perimeter = sides * 2 * radius * Math.sin(Math.PI / sides);
  const sideLen = perimeter / sides;
  return Array.from({ length: count }, (_, i) => {
    const d = (i / count) * perimeter;
    const sideIdx = Math.min(Math.floor(d / sideLen), sides - 1);
    const t = (d - sideIdx * sideLen) / sideLen;
    const from = vertices[sideIdx];
    const to = vertices[(sideIdx + 1) % sides];
    return { id: i, x: Math.round(from.x + t * (to.x - from.x)), y: Math.round(from.y + t * (to.y - from.y)) };
  });
}

export function generateHexagonCities(count: number): City[] {
  const cx = 250, cy = 200, radius = 170;
  const sides = 6;
  const vertices = Array.from({ length: sides }, (_, k) => ({
    x: cx + radius * Math.cos((2 * Math.PI * k) / sides),
    y: cy + radius * Math.sin((2 * Math.PI * k) / sides),
  }));
  const sideLen = radius; // for regular hexagon, side = radius
  const perimeter = sides * sideLen;
  return Array.from({ length: count }, (_, i) => {
    const d = (i / count) * perimeter;
    const sideIdx = Math.min(Math.floor(d / sideLen), sides - 1);
    const t = (d - sideIdx * sideLen) / sideLen;
    const from = vertices[sideIdx];
    const to = vertices[(sideIdx + 1) % sides];
    return { id: i, x: Math.round(from.x + t * (to.x - from.x)), y: Math.round(from.y + t * (to.y - from.y)) };
  });
}
