using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int wood = 0;
    public int raftProgress = 0;

    public TextMeshProUGUI woodText;
    public TextMeshProUGUI raftText;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        woodText.text = "Wood: " + wood;
        raftText.text = "Raft: " + raftProgress + "%";
        Debug.Log("Wood: " + wood);
    }
}