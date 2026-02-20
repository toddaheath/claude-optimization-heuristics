import { useState } from 'react';
import { AlgorithmType, ALGORITHM_LABELS } from '../types';

// ── Shared UI helpers ────────────────────────────────────────────────────────

function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div className="space-y-2">
      <h3 className="text-base font-semibold text-gray-800 border-b border-gray-200 pb-1">{title}</h3>
      <div className="text-sm text-gray-700 leading-relaxed space-y-2">{children}</div>
    </div>
  );
}

interface ParamRowProps {
  name: string;
  type: string;
  default_: string;
  description: string;
  whyDefault: string;
}

function ParamRow({ name, type, default_, description, whyDefault }: ParamRowProps) {
  return (
    <div className="border rounded-lg p-3 bg-gray-50 space-y-1">
      <div className="flex items-center gap-2 flex-wrap">
        <code className="text-blue-700 font-bold text-sm">{name}</code>
        <span className="text-xs text-gray-500 bg-gray-200 rounded px-1">{type}</span>
        <span className="text-xs text-gray-500">default: <strong>{default_}</strong></span>
      </div>
      <p className="text-sm text-gray-700">{description}</p>
      <p className="text-xs text-gray-500 italic">
        <span className="font-semibold not-italic text-gray-600">Why {default_}? </span>
        {whyDefault}
      </p>
    </div>
  );
}

// ── Per-algorithm content ────────────────────────────────────────────────────

function SimulatedAnnealingDoc() {
  return (
    <div className="space-y-5">
      <Section title="How It Works">
        <p>
          Simulated Annealing (SA) is inspired by the metallurgical process of slowly cooling molten
          metal to reach a low-energy crystalline state. For TSP it maintains a <em>current solution</em>{' '}
          and repeatedly proposes a small random change (swapping two cities in the tour). If the
          change improves the distance it is always accepted. If it worsens it, it is still accepted
          with a probability that decreases as the "temperature" falls — allowing the search to escape
          local optima early on while converging to good solutions later.
        </p>
        <p>
          At each iteration the acceptance probability of a worse move is{' '}
          <code>P = exp(−ΔE / T)</code>, where <code>ΔE</code> is the cost increase and <code>T</code>{' '}
          is the current temperature. After every iteration the temperature is multiplied by the
          cooling rate: <code>T ← T × coolingRate</code>.
        </p>
      </Section>

      <Section title="When to Use It">
        <p>
          SA is an excellent general-purpose baseline. It works well on TSP instances of any size,
          requires no population bookkeeping, and is easy to tune. It tends to produce high-quality
          results when allowed a long enough cooling schedule.
        </p>
      </Section>

      <Section title="Parameters">
        <div className="space-y-2">
          <ParamRow
            name="initialTemperature"
            type="number"
            default_="10 000"
            description="The starting 'temperature'. Higher values allow the algorithm to accept bad moves freely at the beginning, enabling exploration of the search space."
            whyDefault="A value in the thousands covers typical TSP distances (which can be hundreds to thousands of units) so the initial acceptance probability for moderately bad moves is close to 1, ensuring thorough early exploration."
          />
          <ParamRow
            name="coolingRate"
            type="number (0–1)"
            default_="0.995"
            description="The geometric factor by which temperature is multiplied each iteration. Values close to 1 cool slowly (more exploration, slower convergence); values closer to 0 cool quickly (fast convergence but risks getting stuck)."
            whyDefault="0.995 gives a good balance for runs of ~500 iterations: the temperature drops to ≈8% of its start, providing meaningful exploration before the search freezes. Increase toward 0.999 for longer runs."
          />
          <ParamRow
            name="minTemperature"
            type="number"
            default_="0.01"
            description="A lower bound on temperature. Once reached, no more bad moves are accepted and the algorithm becomes a greedy hill-climber for the remaining iterations."
            whyDefault="0.01 is low enough that acceptance probability for even tiny cost increases is negligible (e≈0 for any reasonable ΔE), effectively freezing the search without cutting it off prematurely."
          />
        </div>
      </Section>
    </div>
  );
}

function AcoDoc() {
  return (
    <div className="space-y-5">
      <Section title="How It Works">
        <p>
          Ant Colony Optimization (ACO) mimics how real ants deposit pheromone on paths between food
          and the nest. Each simulated ant constructs a complete tour by probabilistically choosing
          the next city based on two factors: the amount of pheromone on that edge (exploitation) and
          the inverse of the edge distance (heuristic desirability). After all ants finish their
          tours, pheromone evaporates slightly and then ants deposit new pheromone on the edges they
          used — proportional to tour quality.
        </p>
        <p>
          Over many iterations shorter edges accumulate more pheromone, biasing future ants toward
          good routes. The evaporation prevents premature convergence by "forgetting" old information.
        </p>
      </Section>

      <Section title="When to Use It">
        <p>
          ACO shines on graph problems where edge quality information is meaningful. It naturally
          handles dynamic changes and can be parallelized. It requires more memory and tuning than SA
          but often finds very good solutions on moderately sized TSP instances.
        </p>
      </Section>

      <Section title="Parameters">
        <div className="space-y-2">
          <ParamRow
            name="antCount"
            type="integer"
            default_="20"
            description="Number of ants per iteration. Each ant constructs one complete tour."
            whyDefault="20 ants provides enough diversity of paths to reinforce good edges without excessive computation per iteration."
          />
          <ParamRow
            name="alpha"
            type="number"
            default_="1.0"
            description="Weight of pheromone in the city-selection probability. Higher values make ants follow existing pheromone more strongly."
            whyDefault="α=1 gives pheromone and heuristic equal initial importance, letting the algorithm discover structure without being blinded by early (poor) pheromone deposits."
          />
          <ParamRow
            name="beta"
            type="number"
            default_="5.0"
            description="Weight of the distance heuristic (1/distance) in city selection. Higher values make ants prefer nearby cities."
            whyDefault="β=5 gives a strong greedy bias toward short edges, which is appropriate for TSP because nearby cities are almost always a good local choice. Without this, early tours are very poor and convergence is slow."
          />
          <ParamRow
            name="evaporationRate"
            type="number (0–1)"
            default_="0.5"
            description="Fraction of pheromone that evaporates each iteration. Higher values forget the past more quickly."
            whyDefault="0.5 is a classic mid-range value: pheromone decays fast enough to avoid stagnation but slowly enough for good solutions to accumulate influence over several iterations."
          />
          <ParamRow
            name="pheromoneDeposit"
            type="number"
            default_="100"
            description="The base amount of pheromone an ant deposits divided by its tour length. Controls the scale of pheromone updates."
            whyDefault="100 is a conventional scaling constant that keeps pheromone levels in a numerically stable range for typical TSP coordinates (0–500 units)."
          />
        </div>
      </Section>
    </div>
  );
}

function GeneticAlgorithmDoc() {
  return (
    <div className="space-y-5">
      <Section title="How It Works">
        <p>
          Genetic Algorithms (GA) maintain a <em>population</em> of candidate tours and evolve it
          over generations using operators inspired by biological evolution:
        </p>
        <ul className="list-disc ml-5 space-y-1">
          <li><strong>Selection:</strong> Parents are chosen via tournament selection — a few random individuals compete and the best wins.</li>
          <li><strong>Crossover (Order Crossover / OX):</strong> A child tour is built by copying a random segment from one parent and filling the rest in the order they appear in the other parent. This preserves valid permutations.</li>
          <li><strong>Mutation:</strong> With a small probability, two cities in a tour are swapped.</li>
          <li><strong>Elitism:</strong> The best few individuals are copied unchanged to the next generation, preventing loss of the current best solution.</li>
        </ul>
      </Section>

      <Section title="When to Use It">
        <p>
          GAs are effective for large, complex search spaces and can maintain diverse solutions.
          They tend to converge more slowly than SA on TSP but can find globally good solutions,
          especially with a large population and many generations.
        </p>
      </Section>

      <Section title="Parameters">
        <div className="space-y-2">
          <ParamRow
            name="populationSize"
            type="integer"
            default_="50"
            description="Number of tour candidates maintained across generations."
            whyDefault="50 individuals provide enough genetic diversity to explore multiple regions of the solution space without making each generation prohibitively slow. Increase for larger problem instances."
          />
          <ParamRow
            name="mutationRate"
            type="number (0–1)"
            default_="0.02"
            description="Probability that any given individual undergoes a random swap mutation each generation."
            whyDefault="2% is a classic rate. Too low and the population stagnates; too high and the GA becomes a random walk. For a 20-city problem, this means roughly 0.4 mutations per individual per generation."
          />
          <ParamRow
            name="tournamentSize"
            type="integer"
            default_="5"
            description="Number of individuals that compete in each selection tournament. Larger values apply stronger selection pressure."
            whyDefault="5 out of 50 (10%) provides moderate selective pressure — good solutions tend to be chosen but not so aggressively that diversity collapses quickly."
          />
          <ParamRow
            name="eliteCount"
            type="integer"
            default_="2"
            description="Number of top-performing individuals copied verbatim to the next generation (elitism)."
            whyDefault="Preserving 2 elite individuals guarantees the best-found solution is never lost while still allowing the rest of the population to explore freely."
          />
        </div>
      </Section>
    </div>
  );
}

function PsoDoc() {
  return (
    <div className="space-y-5">
      <Section title="How It Works">
        <p>
          Particle Swarm Optimization (PSO) is inspired by the flocking behavior of birds. Each
          "particle" represents a candidate tour and has a position (the tour permutation) and a
          velocity. On each step, particles update based on three influences:
        </p>
        <ul className="list-disc ml-5 space-y-1">
          <li><strong>Inertia:</strong> The particle continues in its current direction.</li>
          <li><strong>Cognitive (personal best):</strong> Pull toward the best position the particle itself has ever found.</li>
          <li><strong>Social (global best):</strong> Pull toward the best position any particle in the swarm has ever found.</li>
        </ul>
        <p>
          In the discrete TSP domain, "velocity" is adapted using swap sequences: a velocity is a
          list of swaps applied to produce the new position. New tours are generated by mixing
          elements of the personal and global best solutions.
        </p>
      </Section>

      <Section title="When to Use It">
        <p>
          PSO works well for continuous optimization but requires adaptation for combinatorial problems
          like TSP. It can converge quickly in the early phase but may plateau. It's a good choice
          when you want a simple swarm-based approach with few parameters.
        </p>
      </Section>

      <Section title="Parameters">
        <div className="space-y-2">
          <ParamRow
            name="swarmSize"
            type="integer"
            default_="30"
            description="Number of particles in the swarm."
            whyDefault="30 particles strike a balance between exploration breadth and computation cost. More particles explore more of the space per iteration but each iteration takes longer."
          />
          <ParamRow
            name="cognitiveWeight"
            type="number"
            default_="2.0"
            description="Strength of the pull toward each particle's personal best solution."
            whyDefault="2.0 is a standard PSO coefficient. Equal cognitive and social weights encourage balanced exploration (personal experience) and exploitation (swarm knowledge)."
          />
          <ParamRow
            name="socialWeight"
            type="number"
            default_="2.0"
            description="Strength of the pull toward the global best solution found by any particle."
            whyDefault="Matching cognitiveWeight at 2.0 prevents premature convergence to a single attractor while still benefiting from collective knowledge."
          />
        </div>
      </Section>
    </div>
  );
}

function SlimeMoldDoc() {
  return (
    <div className="space-y-5">
      <Section title="How It Works">
        <p>
          Slime Mold Optimization (SMO) is inspired by the foraging behavior of Physarum polycephalum
          — a single-celled organism that forms efficient transport networks between food sources. The
          algorithm simulates the organism's ability to reinforce successful paths and prune poor ones.
        </p>
        <p>
          Each candidate solution's "weight" (vein thickness) is updated using a sigmoid-like
          function based on its fitness relative to the population. Better solutions grow stronger;
          weaker ones shrink. New candidate tours are generated by combining elements of existing
          high-weight solutions with random perturbations, governed by a parameter{' '}
          <code>z</code> that controls the probability of random exploration versus exploitation.
        </p>
      </Section>

      <Section title="When to Use It">
        <p>
          SMO is a newer biologically-inspired metaheuristic. It tends to converge quickly and can
          find competitive solutions, especially on problems with clear structure. It uses fewer
          parameters than ACO or GA.
        </p>
        <p className="text-amber-700 bg-amber-50 border border-amber-200 rounded p-2 text-xs">
          Note: the <code>Math.Atanh</code> function used internally returns ±Infinity when its
          argument reaches ±1. The implementation clamps values to prevent numerical instability.
        </p>
      </Section>

      <Section title="Parameters">
        <div className="space-y-2">
          <ParamRow
            name="populationSize"
            type="integer"
            default_="30"
            description="Number of candidate slime-mold 'veins' (solutions) maintained."
            whyDefault="30 provides enough diversity for the weight-update mechanism to differentiate good from poor solutions without excessive memory overhead."
          />
          <ParamRow
            name="z"
            type="number (0–1)"
            default_="0.03"
            description="Controls the balance between random exploration and guided exploitation. Higher z means more randomness."
            whyDefault="0.03 (3% randomness) keeps the search mostly exploitation-driven while allowing occasional random jumps to escape local optima. This follows typical SMO paper recommendations."
          />
        </div>
      </Section>
    </div>
  );
}

function TabuSearchDoc() {
  return (
    <div className="space-y-5">
      <Section title="How It Works">
        <p>
          Tabu Search (TS) is a deterministic local search method enhanced with a memory structure
          called the <em>tabu list</em>. At each step, it evaluates a neighborhood of moves (here:
          all 2-opt swaps of pairs of cities) and picks the best non-tabu move, even if it worsens
          the current solution.
        </p>
        <p>
          Moves that were recently made are placed on the tabu list and are forbidden for{' '}
          <code>tabuTenure</code> iterations. This prevents cycling and forces the search to explore
          new regions. An <em>aspiration criterion</em> overrides the tabu prohibition if the move
          would produce a new global best.
        </p>
      </Section>

      <Section title="When to Use It">
        <p>
          Tabu Search is excellent for combinatorial problems and typically outperforms simpler local
          search. It's deterministic (no randomness), making results reproducible. The main cost is
          evaluating a potentially large neighborhood each iteration, so it can be slow on very large
          instances.
        </p>
      </Section>

      <Section title="Parameters">
        <div className="space-y-2">
          <ParamRow
            name="tabuTenure"
            type="integer"
            default_="10"
            description="How many iterations a move remains forbidden after being made."
            whyDefault="10 iterations is a commonly recommended starting point. Too short and cycling recurs; too long and the search is too restricted. A rule of thumb is √n where n is the number of cities."
          />
          <ParamRow
            name="neighborhoodSize"
            type="integer"
            default_="50"
            description="Number of randomly sampled 2-opt neighbor moves evaluated per iteration. The best non-tabu move among them is chosen."
            whyDefault="50 samples a meaningful fraction of the neighborhood for 20-city problems (190 possible 2-opt pairs) without evaluating all of them. Scale up for larger instances."
          />
        </div>
      </Section>
    </div>
  );
}

// ── Tab definitions ──────────────────────────────────────────────────────────

const TABS: { type: AlgorithmType; component: React.ComponentType }[] = [
  { type: AlgorithmType.SimulatedAnnealing,      component: SimulatedAnnealingDoc },
  { type: AlgorithmType.AntColonyOptimization,   component: AcoDoc },
  { type: AlgorithmType.GeneticAlgorithm,        component: GeneticAlgorithmDoc },
  { type: AlgorithmType.ParticleSwarmOptimization, component: PsoDoc },
  { type: AlgorithmType.SlimeMoldOptimization,   component: SlimeMoldDoc },
  { type: AlgorithmType.TabuSearch,              component: TabuSearchDoc },
];

// ── Page ─────────────────────────────────────────────────────────────────────

export function DocumentationPage() {
  const [active, setActive] = useState<AlgorithmType>(AlgorithmType.SimulatedAnnealing);

  const ActiveDoc = TABS.find((t) => t.type === active)?.component ?? SimulatedAnnealingDoc;

  return (
    <div className="max-w-screen-lg mx-auto p-6 space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Algorithm Documentation</h1>
        <p className="text-sm text-gray-500 mt-1">
          How each heuristic works, what its parameters mean, and why the defaults are chosen.
        </p>
      </div>

      {/* Algorithm tabs */}
      <div className="flex flex-wrap gap-2 border-b border-gray-200 pb-3">
        {TABS.map(({ type }) => (
          <button
            key={type}
            onClick={() => setActive(type)}
            className={`px-4 py-1.5 rounded-full text-sm font-medium transition-colors ${
              active === type
                ? 'bg-blue-600 text-white'
                : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
            }`}
          >
            {ALGORITHM_LABELS[type]}
          </button>
        ))}
      </div>

      {/* Content */}
      <div className="bg-white border border-gray-200 rounded-xl p-6 shadow-sm">
        <h2 className="text-xl font-bold text-gray-900 mb-5">{ALGORITHM_LABELS[active]}</h2>
        <ActiveDoc />
      </div>

      {/* General TSP note */}
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 text-sm text-blue-800">
        <p className="font-semibold mb-1">About the Traveling Salesman Problem (TSP)</p>
        <p>
          The TSP asks: given a set of cities and distances between them, find the shortest possible
          tour that visits every city exactly once and returns to the start. It is NP-hard — no known
          polynomial-time algorithm finds the optimal solution for all instances. The heuristics here
          find good (but not necessarily optimal) solutions in reasonable time by intelligently
          searching the enormous space of possible tours (n! permutations for n cities).
        </p>
      </div>
    </div>
  );
}
