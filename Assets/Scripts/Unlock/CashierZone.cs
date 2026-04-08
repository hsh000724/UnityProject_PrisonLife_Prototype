using UnityEngine;

public class CashierZone : UnlockZone
{
    [SerializeField] private Cashier cashier;  // 해금 시 활성화할 캐셔

    protected override string GetUnlockMessage() => "Cashier Unlocked!";

    protected override void OnUnlocked()
    {
        if (cashier != null)
            cashier.gameObject.SetActive(true);
    }
}