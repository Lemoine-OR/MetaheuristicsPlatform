using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Algorithms.Acceptance;

public static class DueckAcceptanceReferences
{
    public static ScientificReference Dueck1993 { get; } = new(
        "Gunter Dueck",
        1993,
        "New Optimization Heuristics: The Great Deluge Algorithm and the Record-to-Record Travel",
        "Journal of Computational Physics 104(1), 86-92",
        "10.1006/jcph.1993.1010");

    public static ScientificReference BurkeBykovNewallPetrovic2003 { get; } = new(
        "Edmund Burke; Yuri Bykov; James Newall; Sanja Petrovic",
        2003,
        "A Time-Predefined Approach to Course Timetabling",
        "Yugoslav Journal of Operations Research 13(2), 139-151",
        "10.2298/YJOR0302139B");

    public static ScientificReference BurkeBykov2016 { get; } = new(
        "Edmund K. Burke; Yuri Bykov",
        2016,
        "An Adaptive Flex-Deluge Approach to University Exam Timetabling",
        "INFORMS Journal on Computing 28(4), 781-794",
        "10.1287/ijoc.2015.0680");
}