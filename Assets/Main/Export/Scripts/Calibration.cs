using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class Calibration : MonoBehaviour
{

    List<Vector3> m_UnityScreenPositions;

    List<Vector3> m_AnalogInputPoints;

    [SerializeField]
    GameObject[] m_CalibrationPointsWithCrosshairs;

    [SerializeField]
    string m_HomographyFile = "/homography_data.txt";
    string m_HomographyFullPath;

    CalcHomography m_HomographyCalculator;

    bool canCalibrate = false;

    int calibratingPointId = 0;

    private void Start()
    {
        m_HomographyCalculator = new CalcHomography();
        m_HomographyFullPath = Application.persistentDataPath + m_HomographyFile;
    }

    public void SetSensorInputPoint(Vector2 inputPoint)
    {
        Vector3 paddedVector = (Vector3)inputPoint;
        m_AnalogInputPoints.Add(paddedVector);
        if(m_AnalogInputPoints.Count == 4)
        {
            CalculateHomography();
        }
        else
        {
            calibratingPointId++;
        }
    }

    public void CalibrateNextPoint()
    {
        m_CalibrationPointsWithCrosshairs[calibratingPointId].SetActive(false);
        m_CalibrationPointsWithCrosshairs[calibratingPointId + 1].SetActive(true);
    }

    public void CalculateHomography()
    {
        List<float>homography = m_HomographyCalculator.CalculateHomography(m_AnalogInputPoints.ToArray(), m_UnityScreenPositions.ToArray());
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

    public void SaveHomography(List<float> homography)
    {
        StreamWriter writer = new StreamWriter(m_HomographyFullPath, true);
        for (int i = 0; i < homography.Count - 1; i++)
        {
            writer.WriteLine(homography[i]);
        }
        writer.Close();
    }
}
