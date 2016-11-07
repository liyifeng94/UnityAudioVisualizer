using UnityEngine;

public class AudioVisualizer : MonoBehaviour
{
    //An AudioSource object so the music can be played .
    private AudioSource _audioSource;

    //Stores channel 0 of the audio samples.
    private float[] _audioSamplesCh0;

    //Stores channel 1 of the audio samples
    private float[] _audioSamplesCh1;

    //Only use channel 0
    public bool MonoChannel = false;

    //Stores the number of audio samples. Min = 64. Max = 8192.
    public int AudioSamplesSize = 64;

    //Height factor of the cube movement
    public float CubeHeightFactor = 15;

    //Horizontal spacing of the cubes  
    public int CubeSpacing = 10;

    //Filter out noise of the data. (Only create a trail cube only if the Y position of the main cube is bigger than this value)
    public int NoiseFliter = 2;

    //A reference to the cube prefab
    public GameObject CubeRef;

    //A reference to the trailing cube prefab
    public GameObject TrailingCubeRef;

    //The length of time the trailing cube will be shown
    public int TrailingCubeDestoryDelay = 10;

    //The transform attached to this game object
    private Transform _transform;

    //An array that stores the transforms of all instantiated cubes
    private Transform[] _cubeTransforms;

    //Line renderer
    private LineRenderer _lineRenderer;

    //The velocity that the cubes will move
    public Vector3 GravityFactor = new Vector3(0.0f, 250.0f, 0.0f);

    //Which FFT function to use
    public FFTWindow FftFunction;

    void Awake()
    {
        //Get and store a reference to the following attached components: 
         
        //AudioSource  
        this._audioSource = GetComponent<AudioSource>();
        //Transform  
        this._transform = GetComponent<Transform>();

        this._lineRenderer = GetComponent<LineRenderer>();
    }

    // Use this for initialization
    void Start()
    {
        this._audioSamplesCh0 = new float[this.AudioSamplesSize];
        this._audioSamplesCh1 = new float[this.AudioSamplesSize];

        //The cubeTransforms array should be initialized double the sample - 1 for the both channels 
        this._cubeTransforms = new Transform[this.AudioSamplesSize * 2 - 1];

        if (this._lineRenderer != null)
        {
            this._lineRenderer.SetVertexCount(_cubeTransforms.Length);
        }

        this._transform.position = new Vector3(this._transform.position.x - this.AudioSamplesSize * this.CubeSpacing, this._transform.position.y, this._transform.position.z);

        //For each cubeTransforms mirrored
        for (var i = 0; i < this._cubeTransforms.Length; ++i)
        {
            //Create a temporary GameObject, that will serve as a reference to the most recent cloned cube and instantiate a cube placing it at the right side of the previous one
            GameObject tempCube = (GameObject)Instantiate(this.CubeRef, new Vector3(this._transform.position.x + i * this.CubeSpacing, this._transform.position.y, this._transform.position.z), Quaternion.identity);

            //Get the recently instantiated cube transform component
            this._cubeTransforms[i] = tempCube.GetComponent<Transform>();

            //Make the cube a child of the game object
            this._cubeTransforms[i].parent = this._transform;
        }


    }

    // Update is called once per frame
    void Update()
    {
        SetSpectrumDataFromSource();

        int midPoint = this.AudioSamplesSize - 1;

        //Update the middle sample;
        float f0;

        if (this.MonoChannel == false)
        {
            f0 = (_audioSamplesCh0[0] + _audioSamplesCh1[0]) / 2;
        }
        else
        {
            f0 = _audioSamplesCh0[0];
        }

        Vector3 cubePos = GetNewCubePosition(midPoint, 0, f0);

        UpdateCubeTransform(cubePos, midPoint);

        //For each sample get the new position
        for (int i = 1; i < this.AudioSamplesSize; ++i)
        {
            //Update left sample
            cubePos = GetNewCubePosition(midPoint - i, i, this._audioSamplesCh0[i]);

            UpdateCubeTransform(cubePos, midPoint - i);


            //Update right sample
            if (this.MonoChannel == false)
            {
                cubePos = GetNewCubePosition(midPoint + i, i, this._audioSamplesCh1[i]);
            }
            else
            {
                cubePos = GetNewCubePosition(midPoint + i, i, this._audioSamplesCh0[i]);
            }

            UpdateCubeTransform(cubePos, midPoint + i);
        }
    }

    void SetSpectrumDataFromSource()
    {
        //Obtain the FFT sample from channel 0 of the frequency bands of the attached AudioSource
        GetSpectrumData(this._audioSource, this._audioSamplesCh0, 0, this.FftFunction);

        if (this.MonoChannel == false)
        {
            //Obtain the FFT sample from channel 1 of the frequency bands of the attached AudioSource
            GetSpectrumData(this._audioSource, this._audioSamplesCh1, 1, this.FftFunction);
        }

    }

    void GetSpectrumData(AudioSource audioSource, float[] audioSample, int channel, FFTWindow fftFunction)
    {
        audioSource.GetSpectrumData(audioSample, channel, fftFunction);
    }

    Vector3 GetNewCubePosition(int index, int sampleIndex, float sampleFft)
    {
        //Set the cubePos Vector3 to the same value as the position of the corresponding cube. However, set it's Y element according to the current sample.
        return new Vector3(this._cubeTransforms[index].position.x, Mathf.Clamp(sampleFft * (50 + sampleIndex * sampleIndex), 0, 50) * CubeHeightFactor + this._transform.position.y, this._cubeTransforms[index].position.z);
    }

    void UpdateCubeTransform(Vector3 cubePos, int cubeIndex)
    {
        if (TrailingCubeRef == null)
        {
            this._cubeTransforms[cubeIndex].position = cubePos;
            return;
        }

        //If the new cubePos.y is greater than the current cube position  
        if (cubePos.y >= this._cubeTransforms[cubeIndex].position.y)
        {
            //Set the cube to the new Y position  
            this._cubeTransforms[cubeIndex].position = cubePos;

            //Spawn trailling cube
            if (cubePos.y >= NoiseFliter)
            {
                CreateTrailingCube(cubePos);
            }
        }
        else if (this._cubeTransforms[cubeIndex].position.y > 0)
        {
            //The spectrum line is below the cube, make it fall  

            this._cubeTransforms[cubeIndex].position -= this.GravityFactor;
        }

        if (_lineRenderer != null)
        {
            this._lineRenderer.SetPosition(cubeIndex, cubePos);
        }
    }

    void CreateTrailingCube(Vector3 cubePos)
    {
        if (TrailingCubeRef == null)
        {
            return;
        }

        //Create a temporary GameObject, that will serve as a reference to the most recent cloned cube and instantiate a cube placing it at the right side of the previous one
        GameObject tempCube = (GameObject)Instantiate(this.TrailingCubeRef, new Vector3(cubePos.x, cubePos.y, cubePos.z), Quaternion.identity);

        Destroy(tempCube, TrailingCubeDestoryDelay);
    }
}
