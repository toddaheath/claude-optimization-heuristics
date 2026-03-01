import { memo } from 'react';
import { AlgorithmType, ALGORITHM_LABELS } from '../types';

interface Props {
  value: AlgorithmType;
  onChange: (type: AlgorithmType) => void;
}

export const AlgorithmSelector = memo(function AlgorithmSelector({ value, onChange }: Props) {
  return (
    <div>
      <label htmlFor="algorithm-select" className="block text-sm font-medium text-gray-700 mb-1">Algorithm</label>
      <select
        id="algorithm-select"
        value={value}
        onChange={(e) => onChange(e.target.value as AlgorithmType)}
        className="w-full px-3 py-2 border rounded-lg text-sm bg-white"
      >
        {Object.values(AlgorithmType).map((type) => (
          <option key={type} value={type}>
            {ALGORITHM_LABELS[type]}
          </option>
        ))}
      </select>
    </div>
  );
});
