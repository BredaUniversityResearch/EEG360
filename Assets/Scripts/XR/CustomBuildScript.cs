using System.Collections;
using System.Collections.Generic;
using UnityEditor.XR.Management;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Management;
using Varjo.XR;
using UnityEngine.XR.OpenXR;
using Unity.XR.Oculus;

public class CustomBuildScript : MonoBehaviour
{
    void BuildForVARJO()
    {
        var generalSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget((BuildTargetGroup)BuildTarget.StandaloneWindows64);
        var settingsManager = generalSettings.Manager;

        // Get example loaders as XRLoaders
        var varjoLoader = new VarjoLoader() as XRLoader;
        var openXRLoader = new OpenXRLoader() as XRLoader;
        var oculusLoader = new OculusLoader() as XRLoader;

        // Adding new loaders
        // Append the new FooLoader
        if (!settingsManager.TryAddLoader(openXRLoader))
            Debug.Log("Adding new Foo Loader failed! Refer to the documentation for additional information!");
        if (!settingsManager.TryAddLoader(oculusLoader))
            Debug.Log("Adding new Foo Loader failed! Refer to the documentation for additional information!");

        // Insert the new BarLoader at the start of the list
        if (!settingsManager.TryAddLoader(varjoLoader, 0))
            Debug.Log("Adding new Bar Loader failed! Refer to the documentation for additional information!");
    }
}
