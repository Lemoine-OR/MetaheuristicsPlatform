using MetaheuristicsPlatform.Neighborhoods;

namespace MetaheuristicsPlatform.Tests;

public sealed class NeighborhoodContractsTests
{
    [Fact]
    public void EnumeratedNeighborhoodCanUseStructCursor()
    {
        var neighborhood =
            new IntegerNeighborhood();

        int solution = 3;

        IntegerMoveEnumerator cursor =
            neighborhood.GetEnumerator(
                in solution);

        Assert.True(
            cursor.MoveNext(
                out int first));

        Assert.True(
            cursor.MoveNext(
                out int second));

        Assert.False(
            cursor.MoveNext(
                out _));

        Assert.Equal(
            -1,
            first);

        Assert.Equal(
            +1,
            second);
    }

    private sealed class IntegerNeighborhood :
        IEnumeratedNeighborhood<
            int,
            int,
            IntegerMoveEnumerator>
    {
        public IntegerMoveEnumerator GetEnumerator(
            in int solution) =>
            new();
    }

    private struct IntegerMoveEnumerator :
        INeighborhoodEnumerator<int>
    {
        private int _position;

        public bool MoveNext(
            out int move)
        {
            switch (_position++)
            {
                case 0:
                    move = -1;
                    return true;

                case 1:
                    move = +1;
                    return true;

                default:
                    move = default;
                    return false;
            }
        }
    }
}