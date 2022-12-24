/*
*   MoveNet
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatML.Examples.Visualizers {

    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using NatML.Vision;
    using NatML.VideoKit.UI;

    /// <summary>
    /// MoveNet body pose visualizer.
    /// This visualizer uses visualizes the pose keypoints using a UI image.
    /// </summary>
    [RequireComponent(typeof(VideoKitCameraView))]
    public sealed class MoveNetVisualizer : MonoBehaviour {

        #region --Inspector--
        [SerializeField]
        public RectTransform keypoint;
        #endregion
        #region --JR Class Vars--
        // class variables stored across multiple calls to Render

        GameObject myGameObject;
        Transform myTransform;
        Animator animator;
        //public int isWalkingHash;
        public int isRunningHash;
        public int RunningAnimationHash;
        //public int isJumpingHash;
        //public int isKickingHash;

        //bool isWalkingParam = false;
        bool isRunningParam = false;
        //bool isJumpingParam = false;
        //bool isKickingParam = false;

        // bool walkPressed = false;      // Walking ==> RAU with a gap after LAU 
        bool runPressed = false;     // Running ==> RAU with small gap after LAU
        // bool jumpPressed = false;     // Jumping ==> both hips 20% above zero position
        // bool kickPressed = false;     // Kicking ==> stage 1 - RAU 25% and in front by 20%  

        int currentFrame = 0;           // frame counter across all frames
        int lauFirstFrame = 1;          // The first frame that saw LeftAnkleUp - set to 1 for init
        int rauFirstFrame = 0;          // The first frame that saw RightAnkleUp 
        int lauLatestFrame = 0;           // The last frame that saw LeftAnkleUp 
        int rauLatestFrame = 0;           // The last frame that saw RightAnkleUp 
        int lauPreRunningFrame = 0;     // The frame that saw LeftAnkleUp while not Running
        int rauPreRunningFrame = 0;     // The frame that saw RightAnkleUp while not Running
        int currentNoAnkleUpSequenceInitFrame = 0;   // The init frame (either lauFirstFrame or rauFirstFrame) of the most recent NoAnkleUp   

        float minAnkleHeightDiff = 0.08f;     // variation between ankle height to trigger Running RAU or LAU section
        float maxTimeGapBetweenAnkleUpsToTriggerRun = 0.6f;    // max time gap between consecutive AnkleUps to trigger Run
        float stopRunIfNoAnkleUpForThisTime = 0.8f;    // if no runningfor 1.2sec then set to stop
        float minAnkleCertaintyForRun = 0.5f;   // the lowest certainty of left and right ankle pose measurements to test for running start
        float maxAnkleHeightVariationDuringNoAnkleUp = 0.08f;  // max variation in ankle height versus init ankle height to detect ankle back down
        float rauFirstFrameLaY = 0f;  // the height of left ankle when RAU first detected - on either init or a subsequent RAU   
        float lauFirstFrameRaY = 0f;  // the height of right ankle when LAU first detected - on either init or a subsequent LAU   
        float minHipCertaintyForTurn = 0.5f;   // the lowest certainty of left hip to test for turning
        float minLeftHipZForLeftTurn = 0.05f;   // least lhZ value to detect user turned left - note lhZ = abs(rhZ) since zero point between hips
        float maxShoulderWidthForTurn = 0.19f;  // max X distance apart that shoulders can be to detect turn
        float minShoulderCertaintyForTurn = 0.5f;   // the lowest certainty of left and right shoulder to detect turn
        //float minHipPercentIncreaseForJump = 1.2f;     // least amount of hip height increase versus zero hip height to detect jump
        //float minHipDropOver5FramesForSquat = 0.15f;
        #endregion

        #region --Client API--
        /// <summary>
        /// Render a body pose.
        /// </summary>
        /// <param name="pose">Body pose to render.</param>
        /// <param name="confidenceThreshold">Keypoints with confidence lower than this value are not rendered.</param>
        public void Render (MoveNetPredictor.Pose pose) {

            currentFrame++;    // JR - toggle frame counter

            //Debug.Log("*** currentFrame = " + currentFrame + " framecount = " + Time.frameCount + " framerate avg= " + Time.frameCount / Time.time);

            // **************************************** Animation stuff from start  ******************************************************

            if (currentFrame == 1)
            {  // do the start stuff
                myGameObject = GameObject.FindGameObjectWithTag("Player");
                myTransform = myGameObject.transform;
                animator = myGameObject.GetComponent<Animator>();
                //isWalkingHash = Animator.StringToHash("isWalking");
                isRunningHash = Animator.StringToHash("isRun");
                RunningAnimationHash = Animator.StringToHash("run");
                //isJumpingHash = Animator.StringToHash("isJumping");
                //isKickingHash = Animator.StringToHash("isKicking");
            }

            // ******************************************* extract pose points into local vars **********************************************************

            int myI = -1;   // jr - local counter

            float laY = 0;
            float laZ = 0;
            float laC = 0;
            float raY = 0;
            float raZ = 0;
            float raC = 0;

            float lsX = 0;
            float lsZ = 0;
            float lsC = 0;
            float rsX = 0;
            float rsZ = 0;
            float rsC = 0;

            float lhZ = 0;
            float lhC = 0;

            float lwX = 0;
            float lwY = 0;
            float lwZ = 0;
            float lwC = 0;
            float rwX = 0;
            float rwY = 0;
            float rwZ = 0;
            float rwC = 0;

            float lkX = 0;
            float lkY = 0;
            float lkZ = 0;
            float lkC = 0;
            float rkX = 0;
            float rkY = 0;
            float rkZ = 0;
            float rkC = 0;


            // Delete current
            foreach (var point in currentPoints)
                GameObject.Destroy(point.gameObject);

            currentPoints.Clear();            
            // Render keypoints

            var imageTransform = transform as RectTransform;
            foreach (var point in pose) {

                  // Instantiate
                //var anchor = Instantiate(keypoint, transform);
                //anchor.gameObject.SetActive(true);
                //// Position
                //anchor.anchorMin = 1f * Vector2.one;
                //anchor.anchorMax = 1f * Vector2.one;
                //anchor.pivot =1f * Vector2.one;
                //anchor.anchoredPosition = Rect.NormalizedToPoint(imageTransform.rect, point);
                //// Add
                //currentPoints.Add(anchor);

                Debug.Log("Point Count:  "+ point);
                // 0 - head, 11,12 - shoulder,    13,14 - elbows,    15,16 - wrists,     23,24 - hips,    25,26 - knees,    27,28 - ankles (left=odd)
                myI++;    //jr

                //leftSoulder
                if (myI == 5)
                {
                    lsX = point[0];
                    lsZ = point[2];
                    var anchor = Instantiate(keypoint, transform);
                    anchor.gameObject.SetActive(true);
                    // Position
                    anchor.anchorMin = 1f * Vector2.one;
                    anchor.anchorMax = 1f * Vector2.one;
                    anchor.pivot = 1f * Vector2.one;
                    anchor.anchoredPosition = Rect.NormalizedToPoint(imageTransform.rect, point);
                    // Add
                    currentPoints.Add(anchor);
                }
                //rightSoulder
                else if (myI == 6)
                {
                    rsX = point[0];
                    rsZ = point[2];
                    var anchor = Instantiate(keypoint, transform);
                    anchor.gameObject.SetActive(true);
                    // Positionsss
                    anchor.anchorMin = 1f * Vector2.one;
                    anchor.anchorMax = 1f * Vector2.one;
                    anchor.pivot = 1f * Vector2.one;
                    anchor.anchoredPosition = Rect.NormalizedToPoint(imageTransform.rect, point);
                    // Add
                    currentPoints.Add(anchor);
                }
                ////leftHip
                else if (myI == 11)
                {
                    lhZ = point[2];
                    var anchor = Instantiate(keypoint, transform);
                    anchor.gameObject.SetActive(true);
                    // Position
                    anchor.anchorMin = 1f * Vector2.one;
                    anchor.anchorMax = 1f * Vector2.one;
                    anchor.pivot = 1f * Vector2.one;
                    anchor.anchoredPosition = Rect.NormalizedToPoint(imageTransform.rect, point);
                    // Add
                    currentPoints.Add(anchor);
                }

                //if (myI == 0)
                //{ //head
                //    //color = Color.black;
                //    //AddKeypoint(point, color);
                //}
                //else if (myI == 11)
                //{   // left shoulders
                //    lsX = point[0];
                //    lsC = point[3];
                //    Color newColor = new Color(0f, 0f, 0.57f);
                //    //color = newColor;
                //    //AddKeypoint(point, color);
                //}
                //else if (myI == 12)
                //{   // right shoulders   
                //    rsX = point[0];
                //    rsC = point[3];
                //    Color newColor = new Color(0f, 0.32f, 1f);
                //    //color = newColor;
                //    //AddKeypoint(point, color);
                //}
                //else if (myI == 15)
                //{   // left wrist
                //    lwX = point[0];
                //    lwY = point[1];
                //    lwZ = point[2];
                //    lwC = point[3];
                //    //color = Color.grey;
                //    //AddKeypoint(point, color);
                //}
                //else if (myI == 16)
                //{   // right wrist
                //    lwX = point[0];
                //    lwY = point[1];
                //    lwZ = point[2];
                //    lwC = point[3];
                //    //color = Color.grey;
                //    //AddKeypoint(point, color);
                //}
                //else if (myI == 23)
                //{   // left hip   
                //    lhZ = point[2];
                //    lhC = point[3];
                //    Color newColor = new Color(0f, 0.54f, 0f);
                //    //color = newColor;
                //    //AddKeypoint(point, color);
                //}
                //else if (myI == 24)
                //{   // right hip   
                //    Color newColor = new Color(0.19f, 0.97f, 0f);
                //    //color = newColor;
                //    //AddKeypoint(point, color);
                //}
                //else if (myI == 25)
                //{   // left knee
                //    lkX = point[0];
                //    lkY = point[1];
                //    lkZ = point[2];
                //    lkC = point[3];
                //    Color newColor = new Color(0.93f, 0.32f, 0.14f);
                //    //color = newColor;
                //    //AddKeypoint(point, color);
                //}
                //else if (myI == 26)
                //{   // right knee
                //    rkX = point[0];
                //    rkY = point[1];
                //    rkZ = point[2];
                //    rkC = point[3];
                //    Color newColor = new Color(0.93f, 0.82f, 0.24f);
                //    //color = newColor;
                //    //AddKeypoint(point, color);
                //}
                //else if (myI == 27)
                //{   // left ankle
                //    laY = point[1];
                //    laZ = point[2];
                //    laC = point[3];
                //    //color = Color.magenta;
                //    //AddKeypoint(point, color);
                //}
                //else if (myI == 28)
                //{   // right ankle
                //    raY = point[1];
                //    raZ = point[2];
                //    raC = point[3];
                //    //color = Color.red;
                //    //AddKeypoint(point, color);
                //}
                //else
                //{
                //    continue;
                //}




            }

            //Debug.Log("laY= " + laY + " raY= " + raY + " laZ= " + laZ + " raZ= " + raZ + " laC= " + laC + " raC= " + raC);
            //Debug.Log("aHeightDiff= " + (raY - laY) + " lhZ= " + lhZ + " lhC= " + lhC + " sDiff= " + (lsX - rsX) + " lsC= " + lsC);

            // ******************************************** Animation stuff from Update **************************************************
            //isWalkingParam = animator.GetBool(isWalkingHash);       // current status of isWalking flag in Animator
            //isRunningParam = animator.GetBool(isRunningHash);       // current status of isRunning flag in Animator
            //isJumpingParam = animator.GetBool(isJumpingHash);       // current status of isJumping flag in Animator
            //isKickingParam = animator.GetBool(isKickingHash);       // current status of isKicking flag in Animator

            //DoRunning(laY, raY, laC, raC);   // detects ankle height difference and measures time between LAU & RAU    
            //DoTurning(lhZ, lhC, lsX, rsX, lsC, rsC);      //  turns character if hips have a Z diff - applied on all frame

            Debug.Log($"lhZ: {lhZ}  lsX:{lsX}  rsX:{rsX}  lsZ:  {lsZ}  rsZ:  {rsZ}");
            NewDoTurning(lhZ, lsX, rsX, lsZ, rsZ);
        }
        #endregion


        #region --Operations--
        private readonly List<RectTransform> currentPoints = new List<RectTransform>();
        #endregion


        public void DoRunning(float laY, float raY, float laC, float raC)
        {
            // RUNNING APPROACH
            // if not running 
            //      if legs chugging (and arms chugging) ===> start running 
            //          TO DO - add a check if arms swinging back n forth?
            // else (running)
            //      if legs chugging  ==> swap to running
            //      else
            //          if 0.3sec without legs chugging ==> stop running 
            //


            // Get variables for running detection
            float rightVsLeftAnkleY = Mathf.Round((raY - laY) * 1000.0f) * 0.001f;
            float ankleCertainty = (laC + raC) / 2;

            Debug.Log("===> isRunningParam= " + isRunningParam + " rightVsLeftAnkleY = " + rightVsLeftAnkleY + " , ankleCertainty= " + ankleCertainty);

            if (isRunningParam is false)
            {           // if not Running                
                if (rightVsLeftAnkleY > minAnkleHeightDiff && ankleCertainty > minAnkleCertaintyForRun)
                {         // if RightAnkleUp with certainty (eg. frame 100)

                    rauPreRunningFrame = currentFrame;    // use on subsequent LAU below to see if RAU/LAU time diff small enuf to trigger walk/run

                    if (((currentFrame - lauPreRunningFrame) * Time.deltaTime) < maxTimeGapBetweenAnkleUpsToTriggerRun)
                    {   // if LAU occurred very recently  ==> 1RAU
                        // nb. it's possible that most recent AnkleUp was also a RAU but it had big gap prior so didnt trigger Running

                        Debug.Log("325 Running or not");
                        runPressed = true;    //  Start Running 
                        rauFirstFrame = currentFrame;  // 1st frame of latest RAU sequence - in this case, the initial sequence - used to identify first frame in each sequence of LAU
                        rauLatestFrame = currentFrame;  // the most recent frame of RAU while running 
                        animator.Play(RunningAnimationHash, 0, 0.157f);   // move ahead to RAU in running animation
                        //animator.Play("idle");
                        //animator.SetBool("isRun", false);
                        //Debug.Log("=========== set init running animation RAU ===========");
                    }

                }
                else if (rightVsLeftAnkleY < -minAnkleHeightDiff && ankleCertainty > minAnkleCertaintyForRun)
                {      // if LAU with certainty (eg. frame 122)

                    lauPreRunningFrame = currentFrame;      // use on subsequent RAU above to see if LAU/RAU time diff small enuf to trigger walk/run

                    if (((currentFrame - rauPreRunningFrame) * Time.deltaTime) < maxTimeGapBetweenAnkleUpsToTriggerRun)
                    {   // if RAU occurred very recently => 1LAU
                        runPressed = true;    //  Start Running 
                        lauFirstFrame = currentFrame;  // 1st frame of latest LAU sequence - in this case, the initial sequence - used to identify first frame in each sequence of RAU
                        lauLatestFrame = currentFrame;       // the most recent frame of LAU while running     
                        animator.Play(RunningAnimationHash, 0, 0.684f);  // move ahead to LAU in running animation
                        //animator.Play("idle");
                        //animator.SetBool("isRun", false);
                        //Debug.Log("=========== set init running animation LAU ===========");
                        // if a long time from earlier RAU to this LAU, we still set the lauPreRunningFrame here to be used on subsequent 
                    }
                }

            }
            else
            {
                Debug.Log("355 running");
                // if Running                      
                // set speed on each sequence of RAU (including first RAU sequence if Running was triggered above via a LAU sequence)
                if (rightVsLeftAnkleY > minAnkleHeightDiff && ankleCertainty > minAnkleCertaintyForRun)
                {   // if RAU with certainty  (eg. frame 144)

                    if (lauFirstFrame > rauFirstFrame)
                    {    // if First Frame of a RAU sequence - either after init LAU sequence or a subsequent LAU
                        rauFirstFrame = currentFrame;      // store first frame of RAU in this RAU sequence 
                        rauFirstFrameLaY = laY; // the height of left ankle when RAU first detected - on this subsequent RAU  - or on init RAU 
                        SetRunningSpeed(lauFirstFrame, rauFirstFrame);  //  adjust speed - based on time between lauFirstFrame and rauFirstFrame (currentFrame)
                        Debug.Log("did SetRunningSpeed - speed set...  lauFirstFrame= " + lauFirstFrame + " rauFirstFrame = " + rauFirstFrame + " speed = " + animator.speed);

                        //animator.Play(RunningAnimationHash, 0, 0.26f);   // jump running animation to just after right ankle lifted
                        //Debug.Log("=========== first RAU frame while running, jump running animation to just after right ankle lifted ===========");                        
                    }
                    rauLatestFrame = currentFrame;   // store latest frame of RAU in latest sequence of RAU - used below to measure time without ankle up to set idle

                }
                else if (rightVsLeftAnkleY < -minAnkleHeightDiff && ankleCertainty > minAnkleCertaintyForRun)
                {   // LAU - no need to set speed but set lauLatestFrame, lauFirstFrame
                    if (rauFirstFrame > lauFirstFrame)
                    {  // if First Frame of a LAU sequence - either after init RAU sequence or a subsequent RAU 
                        lauFirstFrame = currentFrame;      // store first frame of LAU in this LAU sequence 
                        lauFirstFrameRaY = raY; // the height of right ankle when LAU first detected - on this subsequent LAU  - or on init LAU 
                    }
                    lauLatestFrame = currentFrame;   // store latest frame of LAU in latest sequence of LAU - used below to measure time without ankle up to set idle

                }
                else
                {     // if neither ankle is up - adjust animation when ankle first comes back down -  stop Running if timeSinceEitherAnkleUp > 1.2sec 

                    if (rauFirstFrame > lauFirstFrame && ankleCertainty > minAnkleCertaintyForRun)
                    {    // if doing a RAU sequence
                        if (currentNoAnkleUpSequenceInitFrame != rauFirstFrame)
                        {   // if first frame of no ankle up for this RAU sequence
                            if (Mathf.Abs(laY - rauFirstFrameLaY) < maxAnkleHeightVariationDuringNoAnkleUp)
                            {  // laY close to laY at start of this RAU sequence
                                animator.Play(RunningAnimationHash, 0, 0.895f);   // adjust frame of running animation to right ankle down 1/19 * 17=0.895
                                //animator.Play("idle");
                                //animator.SetBool("isRun", false);
                                Debug.Log("### adjust animation to Right Ankle Down ### ");
                            }
                            currentNoAnkleUpSequenceInitFrame = rauFirstFrame;  // so above loop not repeated ie. right ankle first coming back down has occured on this current RAU sequence                   
                        }
                    }
                    else if (lauFirstFrame > rauFirstFrame && ankleCertainty > minAnkleCertaintyForRun)
                    {    // if doing a LAU sequence
                        if (currentNoAnkleUpSequenceInitFrame != lauFirstFrame)
                        {              // if first frame of no ankle up for this LAU sequence
                            if (Mathf.Abs(raY - lauFirstFrameRaY) < maxAnkleHeightVariationDuringNoAnkleUp)
                            {     // raY close to raY at start of this LAU sequence
                                animator.Play(RunningAnimationHash, 0, 0.315f);   // adjust frame of running animation to left ankle down 1/19* 6=0.315
                                //animator.Play("idle");
                                //animator.SetBool("isRun", false);
                                Debug.Log("### adjust animation to Left Ankle Down ### ");
                            }
                            currentNoAnkleUpSequenceInitFrame = lauFirstFrame;  // so above loop not repeated ie. left ankle first coming back down has occured on this current LAU sequence                    
                        }
                    }

                    var timeSinceEitherAnkleUp = (currentFrame - Mathf.Max(lauLatestFrame, rauLatestFrame)) * Time.deltaTime;  // time since either ankle up -
                    Debug.Log("NEITHER ANKLE UP WHILE RUNNING ================ timeSinceEitherAnkleUp = " + timeSinceEitherAnkleUp + " , currentFrame = " + currentFrame);

                    if (timeSinceEitherAnkleUp > stopRunIfNoAnkleUpForThisTime)
                    {    // stop running if >1.8 sec since last ankle up
                        runPressed = false; // set to idle below
                        Debug.Log("IDLE ================ timeSinceEitherAnkleUp = " + timeSinceEitherAnkleUp + " , currentFrame = " + currentFrame);
                    }
                }
            }

            if (isRunningParam && !runPressed)
            {      // if was running but now stop 
                animator.SetBool(isRunningHash, false);
                GameManager.gameManager.characterMove.playerSpeed = 0;
                //  animator.Play("idle");
                //  animator.SetBool("isRun", false);
            }
            else if (!isRunningParam && runPressed)
            {    // if was idle but now running 
                animator.SetBool(isRunningHash, true);
                //animator.Play("run");
                //animator.SetBool("isRun", true);
            }


        }


        // Get time from first frame of prior LAU to first frame of current RAU. Double this to get required speed of running animation loop - prior firstRAU to current firstRAU
        void SetRunningSpeed(int lauFirstFrame, float rauFirstFrame)
        {

            float timeSinceLauFirstFrame = (rauFirstFrame - lauFirstFrame) * Time.deltaTime;  // calc time from lauLatestFrame to rauLatestFrame - ie. first frame of this RAU instance
            float userRunningSpeed = 1 / (2 * timeSinceLauFirstFrame);   // eg. if takes 0.6secs for LAU/RAU then speed = 1/(2*0.6) = 0.8333
            //  if time from prior firstRAU to current firstRAU takes 0.8sec, then 19 frames of running animation need to complete in 0.8sec (ie. animation speed = 0.8)

            // when i lift my right leg, he should have his knee up - not his ankle back

            // problem is we are changing the animation for jsut one frame then it goes back to orig position

            Debug.Log("prior to check, userRunningSpeed = " + userRunningSpeed);

            if (userRunningSpeed < 0.4)
                userRunningSpeed = 0.4f;       // init, too slow, exceptions   --

            if (userRunningSpeed > 1.5)
                userRunningSpeed = 1.5f;       // init, too fast, exceptions  --

            animator.speed = userRunningSpeed;
            GameManager.gameManager.characterMove.playerSpeed = userRunningSpeed;


            /*
            if (timeSinceLauFirstFrame < 0.15) {
                animator.speed = 1.4f;
            } else if (timeSinceLauFirstFrame < 0.25) {
                animator.speed = 1.2f;
            } else if (timeSinceLauFirstFrame < 0.35) {
                animator.speed = 1f;
            } else if (timeSinceLauFirstFrame < 0.55) {
                animator.speed = 0.8f;
            } else if (timeSinceLauFirstFrame < 0.75) {  
                animator.speed = 0.6f;
            } else if (timeSinceLauFirstFrame >= 0.75) {
                    animator.speed = 0.4f;
            }
            */

        }



        void DoTurning(float lhZ, float lhC, float lsX, float rsX, float lsC, float rsC)
        {

            bool turnToLeftPressed = false;
            bool turnToRightPressed = false;
            float shoulderWidth = lsX - rsX;
            float shoulderCertainty = (lsC + rsC) / 2;

            if (lhC > minHipCertaintyForTurn && shoulderCertainty > minShoulderCertaintyForTurn)
            {
                if (lhZ > minLeftHipZForLeftTurn && shoulderWidth < maxShoulderWidthForTurn)
                {         // lhZ back > 10 and shoulders < 0.19 apart ==> left turn
                    turnToLeftPressed = true;
                    //Debug.Log("........turnToRightPressed= " + turnToRightPressed);
                }
                else if (lhZ < -minLeftHipZForLeftTurn && shoulderWidth < maxShoulderWidthForTurn)
                {    // lhZ < -10 and shoulders < 0.19 apart ==> right turn
                    turnToRightPressed = true;
                }
            }
            // incrementally rotate character direction if turnToLeftPressed or turnToRightPressed
            // var directionCharacterIsFacing = myTransform.eulerAngles.y;
            // if (turnToLeftPressed && directionCharacterIsFacing > -60 && directionCharacterIsFacing < 60)  {

            if (turnToLeftPressed)
            {
                //myTransform.eulerAngles = new Vector3(myTransform.eulerAngles.x, myTransform.eulerAngles.y + 3, myTransform.eulerAngles.z);
                myTransform.eulerAngles = new Vector3(myTransform.eulerAngles.x, myTransform.eulerAngles.y - 3, myTransform.eulerAngles.z);
            }
            //else if (turnToRightPressed && directionCharacterIsFacing > -60 && directionCharacterIsFacing < 60)  {
            else if (turnToRightPressed)
            {
                //myTransform.eulerAngles = new Vector3(myTransform.eulerAngles.x, myTransform.eulerAngles.y - 3, myTransform.eulerAngles.z);
                myTransform.eulerAngles = new Vector3(myTransform.eulerAngles.x, myTransform.eulerAngles.y + 3, myTransform.eulerAngles.z);
            }
        }

        void NewDoTurning(float lhZ, float lsX, float rsX,float lsZ,float rsZ)
        {
            Debug.Log("Turn Player");
            bool turnToLeftPressed = false;
            bool turnToRightPressed = false;
            float shoulderWidth = lsX - rsX;
            float shoulderheight = lsZ - rsZ;
            //float shoulderCertainty = (lsC + rsC) / 2;

            //if (lhC > minHipCertaintyForTurn && shoulderCertainty > minShoulderCertaintyForTurn)
            //{
            Debug.Log($"shoulderheight: {shoulderheight}  minLeftHipZForLeftTurn:  {minLeftHipZForLeftTurn}  shoulderWidth:  {shoulderWidth}   maxShoulderWidthForTurn:  {maxShoulderWidthForTurn} ");

                if (shoulderheight < -0.02f && shoulderWidth < maxShoulderWidthForTurn)
                {         // lhZ back > 10 and shoulders < 0.19 apart ==> left turn
                    turnToLeftPressed = true;
                    //Debug.Log("........turnToRightPressed= " + turnToRightPressed);
                }
                else if (shoulderheight >0.02f && shoulderWidth < maxShoulderWidthForTurn)
                {    // lhZ < -10 and shoulders < 0.19 apart ==> right turn
                    turnToRightPressed = true;
                }
            //}
            // incrementally rotate character direction if turnToLeftPressed or turnToRightPressed
            // var directionCharacterIsFacing = myTransform.eulerAngles.y;
            // if (turnToLeftPressed && directionCharacterIsFacing > -60 && directionCharacterIsFacing < 60)  {

            if (turnToLeftPressed)
            {
                //myTransform.eulerAngles = new Vector3(myTransform.eulerAngles.x, myTransform.eulerAngles.y + 3, myTransform.eulerAngles.z);
                myTransform.eulerAngles = new Vector3(myTransform.eulerAngles.x, myTransform.eulerAngles.y - 3, myTransform.eulerAngles.z);
            }
            //else if (turnToRightPressed && directionCharacterIsFacing > -60 && directionCharacterIsFacing < 60)  {
            else if (turnToRightPressed)
            {
                //myTransform.eulerAngles = new Vector3(myTransform.eulerAngles.x, myTransform.eulerAngles.y - 3, myTransform.eulerAngles.z);
                myTransform.eulerAngles = new Vector3(myTransform.eulerAngles.x, myTransform.eulerAngles.y + 3, myTransform.eulerAngles.z);
            }
        }

    }
}