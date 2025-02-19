using UnityEngine;

public class BallInteraction : MonoBehaviour
{
    private TowerOfLondon gameController;
    private Vector3 startPosition;
    private Camera mainCamera;
    private float zOffset;

    void Start()
    {
        gameController = FindObjectOfType<TowerOfLondon>();
        mainCamera = Camera.main;
    }

    void OnMouseDown()
    {
        if (gameController.IsTopBall(gameObject, gameController.FindPegIndex(gameObject)))
        {
            startPosition = transform.position;
            zOffset = mainCamera.WorldToScreenPoint(transform.position).z;
        }
    }

    void OnMouseDrag()
    {
        if (gameController.IsTopBall(gameObject, gameController.FindPegIndex(gameObject)))
        {
            Vector3 screenPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, zOffset);
            Vector3 newPosition = mainCamera.ScreenToWorldPoint(screenPosition);
            transform.position = new Vector3(newPosition.x, transform.position.y, newPosition.z);
        }
    }

    void OnMouseUp()
    {
        if (gameController.IsTopBall(gameObject, gameController.FindPegIndex(gameObject)))
        {
            int closestPeg = FindClosestPeg();
            gameController.MoveBall(gameObject, closestPeg);
        }
    }

    int FindClosestPeg()
    {
        float minDistance = Mathf.Infinity;
        int closestIndex = 0;

        foreach (GameObject peg in gameController.pegs)
        {
            float distance = Vector3.Distance(transform.position, peg.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestIndex = System.Array.IndexOf(gameController.pegs, peg);
            }
        }
        return closestIndex;
    }
}