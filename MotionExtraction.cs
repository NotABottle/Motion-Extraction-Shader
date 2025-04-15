using System;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class MotionExtraction : MonoBehaviour
{
    [System.Serializable]
    public struct Section{
        public Color color;
        public float threshold;
    }

    /*
    Stores the maximum number of previous frames that can be rendered.
    Changing this value above 10 will break the entire shader due to the definite buffer size.
    */
    private const int FRAME_DELAY_LIMIT = 10;

    /*
    This is the renderTexture from which we will be sampling color data from to send to the GPU.
    The renderTexture needs to be set in the inspector from the file in the project directory
    and ensure that the texture is rendered to from whichever camera is attached to the robot's sensor.
    */
    [SerializeField] private RenderTexture sourceRenderTexture;

    /*
    This stores a reference to the material that renders the shader. 
    To use, set this variable in the inspector from either the material attached to this object or 
    from the material in the project directory, and then apply it to any quad or plane.
    */
    [SerializeField] private Material motionExtractionMaterial;

    /*
    This is the resolution of the frames and textures being sampled. Ensure it is consistent with the resolution
    of the renderTexture stored in the project directory.
    */
    [SerializeField] private Vector2 resolution = new Vector2(1920,1080);

    /*
    This color refers to the default color to apply to objects of the most recent frame.
    E.g. the object that is moving will be rendered as this color.
    */
    [SerializeField] private Color maxColor;

    /*
    Array that stores all sections and their respective threshholds to create a gradient of colors across
    the previous frames.
    */
    [SerializeField] private Section[] sections;

    /*
    Allows selection of how many previous frames should be rendered.
    If this value is set to 0 then it will render the base image with no shader effect.
    10 is the maximum possible value due to the definite size of the frame buffer.
    */
    [SerializeField] [Range(0,10)] private int framesOfDelay;

    // [SerializeField] [Range(0,1000)] private int millisecondsOfDelay;

    Texture2D[] textures;
    int currentFrameIndex;

    /*
    At the start of the game, an array of textures is initialized of size equal to the max possible value.
    Then we grab each slot of the array and fill it with a texture and set it to point filter.
    We do this so that we can then later update them.
    Finally we then take all our values and pass them to the shader.
    */
    private void Start(){
        textures = new Texture2D[FRAME_DELAY_LIMIT];
        currentFrameIndex = 0;
        for(int i = 0; i < FRAME_DELAY_LIMIT; i++){
            textures[i] = new Texture2D((int)resolution.x, (int)resolution.y, TextureFormat.ARGB32, false, true);
            textures[i].filterMode = FilterMode.Point;
            textures[i].Apply();
        }

        ApplyShaderParams();
    }

    /*
    Every frame we are grabbing the current frame along with all the current shader settings and passing it to
    the shader to ensure that it is updating whenever we modify a variable.
    */
    private void Update()
    {
        //Updates number of delay frames to render
        ApplyShaderParams();

        //If no frames of delay are requested then the effect will be disabled
        if(framesOfDelay == 0) return;

        //Captures the current frame to update the textures array
        CaptureFrame();

        //Updates overlay frames
        OverlayFrames();

        //Controls which frame needs to be updated
        currentFrameIndex++;
        if(currentFrameIndex >= framesOfDelay){
            currentFrameIndex = 0;
        }
    }
 
    /*
    Updates all parameters existing within the shader. 
    We do this so that the shader can update in real time based on the changes we make to its settings
    */
    private void ApplyShaderParams(){
        motionExtractionMaterial.SetInt("_NumberOfDelayFrames", framesOfDelay);

        Color[] colors = new Color[sections.Length];
        float[] thresholds = new float[sections.Length];

        for(int i = 0; i < sections.Length; i++){
            colors[i] = sections[i].color;
            thresholds[i] = sections[i].threshold;
        }

        motionExtractionMaterial.SetColor("_MaxColor",maxColor);
        motionExtractionMaterial.SetColorArray("_Colors", colors);
        motionExtractionMaterial.SetFloatArray("_Thresholds", thresholds);
        motionExtractionMaterial.SetInt("_NumberOfSections", sections.Length);
    }

    /*
    Updates the frame buffer in the shader with the most recently captured previous frame
    We do this so that the trail of previous frames is kept up to date
    */
    private void OverlayFrames()
    {
        motionExtractionMaterial.SetTexture("_Frame" + currentFrameIndex,textures[currentFrameIndex]);
    }

    /*
    Captures a snapshot of the current frame existing within the render texture so that we can then read the 
    pixel data from it to then send to the GPU
    */
    private void CaptureFrame()
    {
        RenderTexture.active = sourceRenderTexture;
        Texture2D texture2D = textures[currentFrameIndex];
        texture2D.ReadPixels(new Rect(0, 0, (int)resolution.x, (int)resolution.y), 0, 0);
        texture2D.Apply();
    }
}
