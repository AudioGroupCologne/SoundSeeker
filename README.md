# SoundSeeker
Ecologically motivated closed-loop auditory training in VR to improve speech-in-noise perception.

# Run Instructions
For HTC Vive Pro 2, make sure that both Steam and SteamVR are running. HTC Vive Pro requires VivePort to run. 

## Level Fitting Procedure
Procedure to find initial SNR for participants. This has to be started once for every new participant

1. Open *Level Setup* scene
2. Open *SetupManager* game object
3. Enter participant ID and name (this will be used for the result file names but ID is important for file association)
4. Start scene -> Target and distractor will start playing
5. Adjust target level by pressing *1* (louder) and *2* (quieter) keys on keyboard -> Let participant move head around during this procedure
6. Once target is barely audible press space key on keyboard to write configuration
7. Stop the scene

## Training Game Procdure 
Actual training procedure. Instructions are written for use with HTC Vive Pro 2 + HTC controllers. Other HMDs controllers should be immediately usable but other controller buttons may be bound.

1. Open *Training* Scene
2. Open *GameController* game object
3. Important: Enter *participantID* -> If entered wrong, training data will possibly override other participants' data
4. Start the scene
5. Interact with buttons by pointing at them and confirming with controller trigger
6. Confirm target position with controller trigger
7. Ending of the round (default: after 15 targets were found) triggers writing of result data (if scene is stopped before completion, no data is written)

