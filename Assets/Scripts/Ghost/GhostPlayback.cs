using UnityEngine;

public class GhostPlayback : MonoBehaviour
{
    private GhostData ghostData;
    private int currentFrame;
    private bool isPlaying;
    private float playbackTimer;

    private float playbackInterval;

    public void StartPlayback(GhostData data, float interval)
    {
        ghostData = data;
        currentFrame = 0;
        isPlaying = true;
        playbackTimer = 0f;
        playbackInterval = interval * 1.015f;

        // Set initial position and rotation
        if (ghostData.positions.Count > 0)
        {
            transform.position = ghostData.positions[0];
            transform.rotation = ghostData.rotations[0];
        }
    }

    public void StopPlayback()
    {
        isPlaying = false;
        currentFrame = 0;
        ghostData = null;
    }

    private void Update()
    {
        if (!isPlaying || ghostData == null || currentFrame >= ghostData.positions.Count - 1) return;

        // Accumulate time
        playbackTimer += Time.deltaTime;

        // Only advance the frame when enough time has passed
        if (playbackTimer >= playbackInterval)
        {
            playbackTimer -= playbackInterval; // Decrease by interval to allow small discrepancies
            currentFrame++;
        }

        // Interpolate between current and next frame if available
        if (currentFrame < ghostData.positions.Count - 1)
        {
            float t = playbackTimer / playbackInterval; // Interpolation factor (0 to 1)
            transform.position = Vector3.Lerp(ghostData.positions[currentFrame], ghostData.positions[currentFrame + 1], t);
            transform.rotation = Quaternion.Slerp(ghostData.rotations[currentFrame], ghostData.rotations[currentFrame + 1], t);
        }
        else
        {
            // End playback if we reach the last frame
            StopPlayback();
        }
    }
}
