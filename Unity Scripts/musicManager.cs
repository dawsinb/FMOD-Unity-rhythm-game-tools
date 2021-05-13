using UnityEngine;
using System;
using System.Runtime.InteropServices;
using FMODUnity;

// class to handle the playback and retrival of data from an FMOD music event
public class MusicManager : MonoBehaviour {
  // FMOD music event reference
  [SerializeField]
  [EventRef]
  private string music;

  // create timeline info and GCHandle to pin it in memory
  public TimelineInfo timelineInfo = null;
  private GCHandle timelineHandle;

  // music player event reference
  private FMOD.Studio.EventInstance musicPlayEvent;
  // beat call back
  private FMOD.Studio.EVENT_CALLBACK beatCallback;

  // timeline data struct
  [StructLayout(LayoutKind.Sequential)]
  public class TimelineInfo {
    // total length of the song (ms)
    public int length = 0;
    // current position of playback (ms)
    public int position = 0;

    // current beat and bar
    public int beat = 0;
    public int bar = 0;
    // current tempo
    public float tempo = 0;

    // last encountered event marker in the playback
    public string lastMarker = "";
  }


  private void Awake() {
    // initialize the music player
    musicPlayEvent = RuntimeManager.CreateInstance(music);

    // instantiate timelineInfo and pin in memory
    timelineInfo = new TimelineInfo();
    timelineHandle = GCHandle.Alloc(timelineInfo, GCHandleType.Pinned);
    // set user data of our timeline to be our timeline info
    musicPlayEvent.setUserData(GCHandle.ToIntPtr(timelineHandle));

    // instantiate the beatCallback and set the call back for the music play event to it
    beatCallback = new FMOD.Studio.EVENT_CALLBACK(BeatEventCallback);
    musicPlayEvent.setCallback(beatCallback, FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT | FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER);

    //get music event description (static info)
    musicPlayEvent.getDescription(out FMOD.Studio.EventDescription description);
    // record song length
    description.getLength(out timelineInfo.length);
  }

  private void Start() {
    // start the music
    musicPlayEvent.start();
  }

  void OnDestroy() {
    // clean up and end the music play event
    musicPlayEvent.setUserData(IntPtr.Zero);
    musicPlayEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    musicPlayEvent.release();
    // manually free the timeline handle in memory since it was pinned
    timelineHandle.Free();
  }

  private void Update() {
    // update playback position every frame
    musicPlayEvent.getTimelinePosition(out timelineInfo.position);
  }

  // helper function for beat event callbacks
  [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
  private FMOD.RESULT BeatEventCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr) {
    // instantiate the event instance from the given pointer
    FMOD.Studio.EventInstance instance = new FMOD.Studio.EventInstance(instancePtr);

    // check to ensure timelineInfo is properly set up
    IntPtr timelineInfoPtr;
    FMOD.RESULT result = instance.getUserData(out timelineInfoPtr);
    // print error if result errored
    if (result != FMOD.RESULT.OK) {
      Debug.LogError("Timeline Callback error: " + result);
    }
    // ensure info pointer is not null
    else if (timelineInfoPtr != IntPtr.Zero) {
      // get the object to store the event details
      GCHandle timelineHandle = GCHandle.FromIntPtr(timelineInfoPtr);
      TimelineInfo timelineInfo = (TimelineInfo)timelineHandle.Target;

      // check which type of beat event was encountered
      switch (type) {
        // beat event
        case FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT:
          // get parameter details
          var parameter = (FMOD.Studio.TIMELINE_BEAT_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.TIMELINE_BEAT_PROPERTIES));
          // record paramater details
          timelineInfo.beat = parameter.beat;
          timelineInfo.bar = parameter.bar;
          timelineInfo.tempo = parameter.tempo;

          /*
           * insert calls to functions that happen on beat events here
           */
          
          break;
        // marker event
        case FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER:
          // get parameter details
          var parameter = (FMOD.Studio.TIMELINE_MARKER_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.TIMELINE_MARKER_PROPERTIES));
          // record parameter details
          timelineInfo.lastMarker = parameter.name;

          /*
           * insert calls to functions that happen on marker events here
           * use paramater.name to pass the encoded name to the function
           */

          break;
      }
    }
    // return status
    return FMOD.RESULT.OK;
  }

  // debug display for the editor, remove/comment out if unneeded
  #if UNITY_EDITOR
  void OnGUI() {
    var content = "";
    content += $"**Music Manager Debug**\n\n";
    content += $"Length (ms): {timelineInfo.length}, Tempo: {timelineInfo.tempo}\n\n";
    content += $"Playback Position (ms): {timelineInfo.position}\n";
    content += $"Current Bar: {timelineInfo.bar}, Current Beat: {timelineInfo.beat}\n\n";
    content += $"Last Marker: {timelineInfo.lastMarker}";
  
    GUI.Label(new Rect(10,10,300,150), content, GUI.skin.textArea);
  }
  #endif
}
