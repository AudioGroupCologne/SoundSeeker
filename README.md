# SoundSeeker
Ecologically motivated closed-loop auditory training in VR to improve speech-in-noise perception.

# Run Instructions
For HTC Vive Pro 2, make sure that both Steam and SteamVR are running. HTC Vive Pro requires VivePort to run. 

The project is meant to be run as a build. To build it for your machine, follow these steps:
1. Download Unity in the version of the project (2021.3.45f2)
2. Open it and open File -> Build Settings
3. Make sure that the three scenes Menu, LevelSetup and Training are all selected in the "Scenes in Build"-window
4. Press "Build" button

# Study Procedure

## Configuration
On first launch, the build creates `soundseeker_config.json` next to the .exe with two settings:
- `dataRoot` — where participant configs, results, and session data are saved. Defaults to a `Data` folder next to the .exe.
- `backupRoot` — a second location every save is automatically mirrored to. **Empty by default (no backups).** Set this to a separate drive or synced folder before collecting real data.

## First time participant + Level setting procedure
1. Navigate to build folder, run ThesisTrainingGame.exe
2. In the menu (experimenter's screen), select "New Participant"
3. Confirm that you want to create a new participant with new ID by hitting "Confirm" -> best to document the ID and explain to participant here how level fitting is going to work
4. LevelSetup scene will open (takes a while to load)
5. Target stimulus + babble noise will start playing
6.  Adjust target level by pressing *1* (louder) and *2* (quieter) keys on keyboard -> Let participant move head around during this procedure
7.  Once target is barely audible press *space* key on keyboard to write configuration file for participant -> automatically return to menu

Experimenter's note during level setting: The monitor mirrors the headset, so there's no on-screen readout for the experimenter. Target/distractor levels and controls are written to Unity's log instead, at `%USERPROFILE%\AppData\LocalLow\<CompanyName>\ThesisTrainingGame\Player.log` — or launch the .exe with `-logFile -` from a terminal to watch it live.

## Start / continue training procedure
Actual training procedure. Instructions are written for use with HTC Vive Pro 2 + HTC controllers. Other HMDs controllers should be immediately usable but other controller buttons may be bound.

1. Experimenter: Enter participantId in textbox
2. Confirm to start the training -> if the ID doesn't match an existing participant, you'll get a clear error and nothing will start; the training also will not start if the participant has already completed 5 sessions (as per the regimen)
3. Training scene will load (might take a while)
4. Participant can start the session by aiming the controller at the start button and clicking
5. After session is completed (15 targets found) the session ends, Alt + F4 to exit

## Controls    
- Interact with buttons by pointing at them and confirming with controller trigger
- Confirm target position with controller trigger

