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
    const angle = Math.random() * 2 * Math.PI;
    const r = Math.sqrt(Math.random()) * radius;
    return {
      id: i,
      x: Math.round(cx + r * Math.cos(angle)),
      y: Math.round(cy + r * Math.sin(angle)),
    };
  });
}

export function generateSquareCities(count: number): City[] {
  const margin = 40;
  const w = 420, h = 320;
  return Array.from({ length: count }, (_, i) => ({
    id: i,
    x: Math.round(Math.random() * w + margin),
    y: Math.round(Math.random() * h + margin),
  }));
}

export function generateTriangleCities(count: number): City[] {
  const cx = 250, cy = 200, side = 340;
  const th = (Math.sqrt(3) / 2) * side;
  const v0 = { x: cx,            y: cy - (th * 2) / 3 };
  const v1 = { x: cx - side / 2, y: cy + th / 3 };
  const v2 = { x: cx + side / 2, y: cy + th / 3 };
  return Array.from({ length: count }, (_, i) => {
    let r1 = Math.random();
    let r2 = Math.random();
    if (r1 + r2 > 1) {
      r1 = 1 - r1;
      r2 = 1 - r2;
    }
    const x = (1 - r1 - r2) * v0.x + r1 * v1.x + r2 * v2.x;
    const y = (1 - r1 - r2) * v0.y + r1 * v1.y + r2 * v2.y;
    return { id: i, x: Math.round(x), y: Math.round(y) };
  });
}

function isInsidePolygon(x: number, y: number, vertices: {x: number, y: number}[]): boolean {
  let inside = false;
  for (let i = 0, j = vertices.length - 1; i < vertices.length; j = i++) {
    const xi = vertices[i].x, yi = vertices[i].y;
    const xj = vertices[j].x, yj = vertices[j].y;
    if ((yi > y) !== (yj > y) && x < (xj - xi) * (y - yi) / (yj - yi) + xi) {
      inside = !inside;
    }
  }
  return inside;
}

export function generatePentagonCities(count: number): City[] {
  const cx = 250, cy = 200, radius = 175;
  const sides = 5;
  const vertices = Array.from({ length: sides }, (_, k) => ({
    x: cx + radius * Math.cos((2 * Math.PI * k) / sides - Math.PI / 2),
    y: cy + radius * Math.sin((2 * Math.PI * k) / sides - Math.PI / 2),
  }));

  const minX = Math.min(...vertices.map(v => v.x));
  const maxX = Math.max(...vertices.map(v => v.x));
  const minY = Math.min(...vertices.map(v => v.y));
  const maxY = Math.max(...vertices.map(v => v.y));

  const cities: City[] = [];
  while (cities.length < count) {
    const x = Math.random() * (maxX - minX) + minX;
    const y = Math.random() * (maxY - minY) + minY;
    if (isInsidePolygon(x, y, vertices)) {
      cities.push({ id: cities.length, x: Math.round(x), y: Math.round(y) });
    }
  }
  return cities;
}

export function generateHexagonCities(count: number): City[] {
  const cx = 250, cy = 200, radius = 170;
  const sides = 6;
  const vertices = Array.from({ length: sides }, (_, k) => ({
    x: cx + radius * Math.cos((2 * Math.PI * k) / sides),
    y: cy + radius * Math.sin((2 * Math.PI * k) / sides),
  }));

  const minX = Math.min(...vertices.map(v => v.x));
  const maxX = Math.max(...vertices.map(v => v.x));
  const minY = Math.min(...vertices.map(v => v.y));
  const maxY = Math.max(...vertices.map(v => v.y));

  const cities: City[] = [];
  while (cities.length < count) {
    const x = Math.random() * (maxX - minX) + minX;
    const y = Math.random() * (maxY - minY) + minY;
    if (isInsidePolygon(x, y, vertices)) {
      cities.push({ id: cities.length, x: Math.round(x), y: Math.round(y) });
    }
  }
  return cities;
}
