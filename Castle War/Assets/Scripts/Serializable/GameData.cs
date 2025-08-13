using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    public string selectedMapID;
    public int woodAmount;
    public int meatAmount;
    public int goldAmount;
    public int armyAmount;
    public int[] playerDropValues;
    public int[] positionDropValues;

    public GameData()
    {
        selectedMapID = "";
        woodAmount = 12500;
        meatAmount = 12500;
        goldAmount = 12500;
        armyAmount = 6;

        playerDropValues = new int[10];
        positionDropValues = new int[10];
    }
}
