using UnityEngine;

public static class TestSession
{
    public static int SelectedTestId { get; private set; }

    public static void SelectTest(int testId)
    {
        SelectedTestId = testId;
    }
}
