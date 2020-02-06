using System;
using System.Collections.Generic;
using ExcelConfig;
using UnityEngine;
using XNet.Libs.Utility;

public class UnityLoger : Loger
{
    #region implemented abstract members of Loger
    public override void WriteLog(DebugerLog log)
    {
        switch (log.Type)
        {
            case LogerType.Error:
                Debug.LogError(log);
                break;
            case LogerType.Log:
                Debug.Log(log);
                break;
            case LogerType.Waring:
            case LogerType.Debug:
                Debug.LogWarning(log);
                break;
        }

    }
    #endregion
}