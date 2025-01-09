using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Play.Review;

public class AppReview : MonoBehaviour
{
    //app review
    private ReviewManager _reviewManager;
    private PlayReviewInfo _playReviewInfo;
    
    public void RequestReview() {
#if !UNITY_EDITOR
        StartCoroutine(RequestAppReview()); //request the review
        
#endif
        Debug.Log("REVIEW REQUESTED");
    }

    IEnumerator RequestAppReview() {
        _reviewManager = new ReviewManager();

        //request a reviewinfo object
        var requestFlowOperation = _reviewManager.RequestReviewFlow();
        yield return requestFlowOperation;
        if (requestFlowOperation.Error != ReviewErrorCode.NoError) {
            // Log error. For example, using requestFlowOperation.Error.ToString().
            yield break;
        }

        _playReviewInfo = requestFlowOperation.GetResult();

        //launch the inapp review flow
        var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);
        yield return launchFlowOperation;
        _playReviewInfo = null; // Reset the object
        if (launchFlowOperation.Error != ReviewErrorCode.NoError) {
            // Log error. For example, using requestFlowOperation.Error.ToString().
            yield break;
        }
        // The flow has finished. The API does not indicate whether the user
        // reviewed or not, or even whether the review dialog was shown. Thus, no
        // matter the result, we continue our app flow.
    }
}
