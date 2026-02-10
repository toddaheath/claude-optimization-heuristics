import { AlgorithmType, ALGORITHM_LABELS } from '../types';

interface Props {
  value: AlgorithmType;
  onChange: (type: AlgorithmType) => void;
}

export function AlgorithmSelector({ value, onChange }: Props) {
  return (
    <div>
      <label className="block text-sm font-medium text-gray-700 mb-1">Algorithm</label>
      <select
        value={value}
        onChange={(e) => onChange(e.target.value as AlgorithmType)}
        className="w-full px-3 py-2 border rounded-lg text-sm"
      >
        {Object.values(AlgorithmType).map((type) => (
          <option key={type} value={type}>
            {ALGORITHM_LABELS[type]}
          </option>
        ))}
      </select>
    </div>
  );
}
