using UnityEngine;

[CreateAssetMenu(fileName = "PrisonerData", menuName = "Prison/PrisonerData")]
public class PrisonerData : ScriptableObject
{
    public int demandMin = 5;
    public int demandMax = 15;
    public int moneyPerHandcuff = 2;
}