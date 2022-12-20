using System.Text.RegularExpressions;

class Valve
{
    public static int usefulValvesCounter = 0;
    public List<int> neighbors;

    private int usefulId;
    public int UsefulId { get => usefulId; }
    private int tag;
    public int Tag { get => tag; }
    private int pressure;
    public int Pressure { get => pressure; }
    public Valve(string tagStr, int pressure)
    {
        this.pressure = pressure;
        usefulId = -1;
        if (this.pressure > 0)
        {
            usefulId = (1 << usefulValvesCounter);
            usefulValvesCounter++;
        }
        this.tag = valveInt(tagStr);
        neighbors = new List<int>();
    }

    public void AddNeighbor(string neigh)
    {
        neighbors.Add(valveInt(neigh));
    }

    public static int valveInt(string s)
    {
        int v = (int)s[0];
        return (v << 8) + (int)s[1];
    }

    public static long valveState(int visited, int ntagYou, int ntagElephant)
    {
        long state = ntagYou;
        state <<= 32;
        state |= ((long)ntagElephant << 16);
        return state | (long)visited;
    }

    public static void valveVisitedTag(long state, out int visited, out int ntagYou, out int ntagElephant)
    {
        visited = (int)(0xffff & state);
        ntagElephant = (int)(0xffff & (state >> 16));
        ntagYou = (int)(state >> 32);
    }
}

internal class Program
{

    private static void Main(string[] args)
    {
        Dictionary<int, Valve> valvesDict = new Dictionary<int, Valve>();
        string[] lines = File.ReadAllLines(@"input.txt");
        Regex rx = new Regex(@"^Valve ([A-Z]{2}) has flow rate=(\d+); tunnels? leads? to valves? ([A-Z]{2}){1}(?:, ([A-Z]{2}))*",
          RegexOptions.Compiled);

        foreach (string line in lines)
        {

            GroupCollection groups = rx.Match(line).Groups;

            Valve valve = new Valve(groups[1].Value, int.Parse(groups[2].Value));
            valvesDict[valve.Tag] = valve;
            valve.AddNeighbor(groups[3].Value);
            foreach (var s in groups[4].Captures)
            {
                valve.AddNeighbor(s.ToString());
            }

        }

        int start = Valve.valveInt("AA");


        Dictionary<long, long> statePressCurr, statePressPrev;
        statePressCurr = new Dictionary<long, long>();
        statePressCurr[Valve.valveState(0, start, start)] = 0;
        int visited, newVisited, ntagYou, ntagElephant;
        long state, currPresure, newPressure;
        for (long k = 25; k > 0; k--)
        {
            Console.WriteLine($"{k}:{statePressCurr.Count}");
            statePressPrev = statePressCurr;
            statePressCurr = new Dictionary<long, long>();
            foreach (var entry in statePressPrev)
            {
                Valve.valveVisitedTag(entry.Key, out visited, out ntagYou, out ntagElephant);
                Valve valve1 = valvesDict[ntagYou];
                Valve valve2 = valvesDict[ntagElephant];
                newPressure = entry.Value;
                foreach (int n1 in valve1.neighbors)
                {
                    foreach (int n2 in valve2.neighbors)
                    {
                        state = Valve.valveState(visited, n1, n2);
                        if (!statePressCurr.TryGetValue(state, out currPresure) || newPressure > currPresure)
                        {
                            statePressCurr[state] = newPressure;
                        }
                    }
                }

                if (valve1.UsefulId != -1 && (valve1.UsefulId & visited) == 0)
                {
                    newPressure = entry.Value + valve1.Pressure * k;
                    newVisited = visited | valve1.UsefulId;
                }
                else
                {
                    newPressure = entry.Value;
                    newVisited = visited;
                }

                foreach (int n2 in valve2.neighbors)
                {
                    state = Valve.valveState(newVisited, ntagYou, n2);
                    if (!statePressCurr.TryGetValue(state, out currPresure) || newPressure > currPresure)
                    {
                        statePressCurr[state] = newPressure;
                    }
                }

                if (valve2.UsefulId != -1 && (valve2.UsefulId & visited) == 0)
                {
                    newPressure = entry.Value + valve2.Pressure * k;
                    newVisited = visited | valve2.UsefulId;
                }
                else
                {
                    newPressure = entry.Value;
                    newVisited = visited;
                }

                foreach (int n1 in valve1.neighbors)
                {
                    state = Valve.valveState(newVisited, n1, ntagElephant);
                    if (!statePressCurr.TryGetValue(state, out currPresure) || newPressure > currPresure)
                    {
                        statePressCurr[state] = newPressure;
                    }
                }


                newPressure = entry.Value;
                newVisited = visited;

                if (valve1.UsefulId != -1 && (valve1.UsefulId & newVisited) == 0)
                {
                    newPressure += valve1.Pressure * k;
                    newVisited |= valve1.UsefulId;
                }

                if (valve2.UsefulId != -1 && (valve2.UsefulId & newVisited) == 0)
                {
                    newPressure += valve2.Pressure * k;
                    newVisited |= valve2.UsefulId;
                }

                state = Valve.valveState(newVisited, ntagYou, ntagElephant);
                if (!statePressCurr.TryGetValue(state, out currPresure) || newPressure > currPresure)
                {
                    statePressCurr[state] = newPressure;
                }
            }
        }

        long maxVal = -1;
        foreach (var entry in statePressCurr)
        {
            if (entry.Value > maxVal)
                maxVal = entry.Value;
        }

        Console.WriteLine(maxVal);

        return;

    }
}