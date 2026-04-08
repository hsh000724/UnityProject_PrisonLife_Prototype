using System;
using UnityEngine;

public class OreObject : MonoBehaviour
{
    public event Action<OreObject> OnMined;
    public bool IsMining { get; private set; }

    private void OnEnable()
    {
        IsMining = false;
    }

    public void Mine()
    {
        if (IsMining) return;
        IsMining = true;
        OnMined?.Invoke(this);
        SoundManager.Instance?.PlayWithRandomPitch(SoundManager.SFX.OreBreak);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsMining) return;

        // Player Ã¤±¤
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerMining>()?.TryStartMining(this);
            return;
        }

        // NPC (±¤ºÎ) Ã¤±¤
        if (other.CompareTag("NPC"))
        {
            other.GetComponent<Miner>()?.TryStartMining(this);
        }
    }
}