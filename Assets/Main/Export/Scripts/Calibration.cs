using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;

public class Calibration : MonoBehaviour
{

    List<Vector3> m_UnityScreenPositions;

    List<Vector3> m_AnalogInputPoints;

    List<Vector2> m_TemporaryPointsCollection;

    int m_MaxNumOfPointsInCollection = 10;

    [SerializeField]
    GameObject[] m_CalibrationPointsWithCrosshairs;

    [SerializeField]
    string m_HomographyFile = "/homography_data.txt";
    string m_HomographyFullPath;

    CalcHomography m_HomographyCalculator;

    bool canCalibrate = false;

    int calibratingPointId = 0;

    public System.Action OnCalibrationFinished;

    private void Awake()
    {
        m_AnalogInputPoints = new List<Vector3>();
        m_UnityScreenPositions = new List<Vector3>();
        m_TemporaryPointsCollection = new List<Vector2>();
    }

    private void Start()
    {
        m_HomographyCalculator = new CalcHomography();
        m_HomographyCalculator.InitiateDevice();
        m_HomographyFullPath = Application.persistentDataPath + m_HomographyFile;



        for (int i = 0; i < m_CalibrationPointsWithCrosshairs.Length; i++)
        {
            m_UnityScreenPositions.Add(m_CalibrationPointsWithCrosshairs[i].transform.position);
            if (i > 0) m_CalibrationPointsWithCrosshairs[i].SetActive(false);
        }
    }

    public void RedoCalibration()
    {
        m_AnalogInputPoints.Clear();
        calibratingPointId = 0;
        m_CalibrationPointsWithCrosshairs[0].SetActive(true);
    }

    public void SetSensorInputPoint()
    {
        //Remove the lower half of the collected points
        int index = m_TemporaryPointsCollection.Count / 2;
        m_TemporaryPointsCollection.RemoveRange(index, m_TemporaryPointsCollection.Count - index);

        // Average the remaining points
        var avg = new Vector2(
        m_TemporaryPointsCollection.Average(x => x.x),
        m_TemporaryPointsCollection.Average(x => x.y));

        Vector3 paddedVector = avg;

        m_AnalogInputPoints.Add(paddedVector);

        // Clear the point collection for the next calibration point
        m_TemporaryPointsCollection.Clear();

        if (m_AnalogInputPoints.Count == 4)
        {
            m_CalibrationPointsWithCrosshairs[calibratingPointId].SetActive(false);
            CalculateHomography();
        }
        else if (m_AnalogInputPoints.Count < 4)
        {
            CalibrateNextPoint();
            calibratingPointId++;
        }
        else
        {
            Debug.Log("More than 4 points detected !!!");
        }
    }

    public void CalibrateNextPoint()
    {
        m_CalibrationPointsWithCrosshairs[calibratingPointId].SetActive(false);
        m_CalibrationPointsWithCrosshairs[calibratingPointId + 1].SetActive(true);
    }

    public void CalculateHomography()
    {
        float[] homography = m_HomographyCalculator.CalculateHomography(m_AnalogInputPoints.ToArray(), m_UnityScreenPositions.ToArray());
        SaveHomography(homography);
    }

    public void LoadHomographyMatrix(string path)
    {
        string prefix = Application.persistentDataPath;
        string fullPath = prefix + path;

        if (File.Exists(fullPath))
        {

            string text = "init";

            StreamReader reader = new StreamReader(path);
            List<float> homographyFromFile = new List<float>();
            while (text != null)
            {
                try
                {
                    text = reader.ReadLine();
                    if (text != null)
                    {
                        float val = float.Parse(text, CultureInfo.InvariantCulture);
                        homographyFromFile.Add(val);
                        Debug.Log("val : " + val);
                    }
                }
                catch (FormatException exc)
                {
                    Debug.Log("FormatException : " + exc);
                }
            }
            reader.Close();
        }
    }

    public void SaveHomography(float[] homography)
    {
        if (File.Exists(m_HomographyFullPath))
        {
            File.Delete(m_HomographyFullPath);
        }
        StreamWriter writer = new StreamWriter(m_HomographyFullPath, true);
        for (int i = 0; i < homography.Length; i++)
        {
            writer.WriteLine(homography[i]);
        }
        writer.Close();
        OnCalibrationFinished.Invoke();
    }

    public void SetMaxNumOfPointsInCollection(int num)
    {
        m_MaxNumOfPointsInCollection = num;
    }

    public void AccumulatePoints(Vector2 point)
    {
        m_TemporaryPointsCollection.Add(point);
        if(m_TemporaryPointsCollection.Count > m_MaxNumOfPointsInCollection)
        {
            m_TemporaryPointsCollection.RemoveAt(0);
        }
    }
}
