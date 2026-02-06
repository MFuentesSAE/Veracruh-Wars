using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int score = 0;
    [SerializeField] private int lives = 3; // Player starts with 3 lives

    void Start()
    {
        lives = 3; // Initialize lives at the start of the game
        score = 0; // Initialize score at the start of the game
    }

    void Update()
    {
        
    }
}
