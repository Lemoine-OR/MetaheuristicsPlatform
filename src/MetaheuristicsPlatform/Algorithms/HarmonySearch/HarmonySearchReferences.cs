using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.HarmonySearch;

/// <summary>
/// Scientific provenance for the canonical Harmony Search foundation and explicitly
/// separated later variants.
/// </summary>
public static class HarmonySearchReferences
{
    public static ScientificReference GeemKimLoganathan2001 { get; } =
        new(
            "Z. W. Geem, J. H. Kim, G. V. Loganathan",
            2001,
            "A New Heuristic Optimization Algorithm: Harmony Search",
            "SIMULATION 76(2), 60-68",
            "10.1177/003754970107600201");

    public static ScientificReference MahdaviFesangharyDamangir2007 { get; } =
        new(
            "M. Mahdavi, M. Fesanghary, E. Damangir",
            2007,
            "An improved harmony search algorithm for solving optimization problems",
            "Applied Mathematics and Computation 188(2), 1567-1579",
            "10.1016/j.amc.2006.11.033");

    public static ScientificReference OmranMahdavi2008 { get; } =
        new(
            "M. G. H. Omran, M. Mahdavi",
            2008,
            "Global-best harmony search",
            "Applied Mathematics and Computation 198(2), 643-656",
            "10.1016/j.amc.2007.09.004");
    public static ScientificReference PanSuganthanTasgetirenLiang2010 { get; } =
        new(
            "Q.-K. Pan, P. N. Suganthan, M. F. Tasgetiren, J. J. Liang",
            2010,
            "A self-adaptive global best harmony search algorithm for continuous optimization problems",
            "Applied Mathematics and Computation 216(3), 830-848",
            "10.1016/j.amc.2010.01.088");
    public static ScientificReference ZouGaoWuLi2010NovelGlobal { get; } =
        new(
            "D. X. Zou, L. Q. Gao, J. H. Wu, S. Li, Y. Li",
            2010,
            "A novel global harmony search algorithm for reliability problems",
            "Computers & Industrial Engineering 58(2), 307-316",
            "10.1016/j.cie.2009.11.003");

    public static ScientificReference ZouGaoWuLi2010Unconstrained { get; } =
        new(
            "D. Zou, L. Gao, J. Wu, S. Li",
            2010,
            "Novel global harmony search algorithm for unconstrained problems",
            "Neurocomputing 73(16-18), 3308-3318",
            "10.1016/j.neucom.2010.07.010");
    public static ScientificReference GeemSim2010ParameterSettingFree { get; } =
        new(
            "Z. W. Geem, K.-B. Sim",
            2010,
            "Parameter-setting-free harmony search algorithm",
            "Applied Mathematics and Computation 217(8), 3881-3889",
            "10.1016/j.amc.2010.09.049");
    public static ScientificReference JeongParkGeemSim2020AdvancedParameterSettingFree { get; } =
        new(
            "Y.-W. Jeong, S.-M. Park, Z. W. Geem, K.-B. Sim",
            2020,
            "Advanced Parameter-Setting-Free Harmony Search Algorithm",
            "Applied Sciences 10(7), 2586",
            "10.3390/app10072586");
}