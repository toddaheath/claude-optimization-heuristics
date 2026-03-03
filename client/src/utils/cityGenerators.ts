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
  return Array.from({ length: count }, (_, i) => {
    const angle = (2 * Math.PI * i) / count;
    return {
      id: i,
      x: Math.round(cx + radius * Math.cos(angle)),
      y: Math.round(cy + radius * Math.sin(angle)),
    };
  });
}

export function generateSquareCities(count: number): City[] {
  const margin = 40;
  const w = 420, h = 320;
  const perimeter = 2 * (w + h);
  const step = perimeter / count;

  return Array.from({ length: count }, (_, i) => {
    const d = step * i;
    let x: number, y: number;
    if (d < w) {
      x = margin + d;
      y = margin;
    } else if (d < w + h) {
      x = margin + w;
      y = margin + (d - w);
    } else if (d < 2 * w + h) {
      x = margin + w - (d - w - h);
      y = margin + h;
    } else {
      x = margin;
      y = margin + h - (d - 2 * w - h);
    }
    return { id: i, x: Math.round(x), y: Math.round(y) };
  });
}

function distributeOnPolygonEdges(
  vertices: { x: number; y: number }[],
  count: number,
): City[] {
  const edgeLengths = vertices.map((v, i) => {
    const next = vertices[(i + 1) % vertices.length];
    return Math.hypot(next.x - v.x, next.y - v.y);
  });
  const perimeter = edgeLengths.reduce((a, b) => a + b, 0);
  const step = perimeter / count;

  const cities: City[] = [];
  for (let i = 0; i < count; i++) {
    let d = step * i;
    for (let e = 0; e < vertices.length; e++) {
      if (d <= edgeLengths[e] || e === vertices.length - 1) {
        const next = vertices[(e + 1) % vertices.length];
        const t = d / edgeLengths[e];
        cities.push({
          id: i,
          x: Math.round(vertices[e].x + t * (next.x - vertices[e].x)),
          y: Math.round(vertices[e].y + t * (next.y - vertices[e].y)),
        });
        break;
      }
      d -= edgeLengths[e];
    }
  }
  return cities;
}

export function generateTriangleCities(count: number): City[] {
  const cx = 250, cy = 200, side = 340;
  const th = (Math.sqrt(3) / 2) * side;
  const vertices = [
    { x: cx,            y: cy - (th * 2) / 3 },
    { x: cx - side / 2, y: cy + th / 3 },
    { x: cx + side / 2, y: cy + th / 3 },
  ];
  return distributeOnPolygonEdges(vertices, count);
}

export function generatePentagonCities(count: number): City[] {
  const cx = 250, cy = 200, radius = 175;
  const sides = 5;
  const vertices = Array.from({ length: sides }, (_, k) => ({
    x: cx + radius * Math.cos((2 * Math.PI * k) / sides - Math.PI / 2),
    y: cy + radius * Math.sin((2 * Math.PI * k) / sides - Math.PI / 2),
  }));
  return distributeOnPolygonEdges(vertices, count);
}

export function generateHexagonCities(count: number): City[] {
  const cx = 250, cy = 200, radius = 170;
  const sides = 6;
  const vertices = Array.from({ length: sides }, (_, k) => ({
    x: cx + radius * Math.cos((2 * Math.PI * k) / sides),
    y: cy + radius * Math.sin((2 * Math.PI * k) / sides),
  }));
  return distributeOnPolygonEdges(vertices, count);
}
