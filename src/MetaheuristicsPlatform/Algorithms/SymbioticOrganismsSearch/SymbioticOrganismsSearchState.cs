namespace MetaheuristicsPlatform.Algorithms.SymbioticOrganismsSearch;
public enum SymbioticOrganismsSearchPhase { Initialization=0, Mutualism=1, Commensalism=2, Parasitism=3, CompletedIteration=4 }
public readonly record struct SymbioticOrganismsSearchState(int Iteration, SymbioticOrganismsSearchPhase Phase, int OrganismIndex, double? BestFitness);
