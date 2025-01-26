using UnityEngine;

public class PlanetSimulator : MonoBehaviour  
{  
    public LineRenderer trajectoryLine;  
    public float simulationSpeed = 1.0f;  
    private Vector3[] _predictedPositions;
    private Gradient _velocityGradient;
    private int _currentIndex;

    void Start()  
    {
        InitializeGradient();
        InitializeTrajectory();
        StartCoroutine(MovePlanet());
    }

    void InitializeGradient()
    {
        _velocityGradient = new Gradient();  
        _velocityGradient.SetKeys(  
            new[] { 
                new GradientColorKey(Color.red, 0),  
                new GradientColorKey(Color.blue, 1)  
            },  
            new[] { 
                new GradientAlphaKey(1, 0),  
                new GradientAlphaKey(1, 1)  
            }  
        );
    }

    void InitializeTrajectory()
    {
        trajectoryLine.positionCount = _predictedPositions.Length;
        trajectoryLine.SetPositions(_predictedPositions);
        trajectoryLine.colorGradient = _velocityGradient;
    }

    IEnumerator MovePlanet()  
    {  
        for (int i = 0; i < _predictedPositions.Length - 1; i++)  
        {  
            float t = 0;
            Vector3 startPos = _predictedPositions[i];
            Vector3 endPos = _predictedPositions[i + 1];
            
            while (t < 1)
            {
                t += Time.deltaTime * simulationSpeed;
                transform.position = Vector3.Slerp(startPos, endPos, t);
                UpdateTrajectoryColor(i, t);
                yield return null;
            }
        }  
    }

    void UpdateTrajectoryColor(int segmentIndex, float progress)
    {
        Gradient segmentGradient = new Gradient();
        segmentGradient.SetKeys(
            new[] {
                new GradientColorKey(trajectoryLine.colorGradient.Evaluate(segmentIndex), 0),
                new GradientColorKey(trajectoryLine.colorGradient.Evaluate(segmentIndex + 1), 1)
            },
            new[] {
                new GradientAlphaKey(1, 0),
                new GradientAlphaKey(1, 1)
            }
        );
        trajectoryLine.colorGradient = segmentGradient;
    }
}