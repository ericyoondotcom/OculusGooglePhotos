using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginScene : MonoBehaviour
{
    [TextArea(3, 10)]
    public string textTemplate;
    public TextMeshProUGUI textBox;

    string linkCode;

    static string GenerateLinkCode()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, 9).Select(s => s[Random.Range(0, s.Length)]).ToArray());
    }

    public void Start()
    {
        linkCode = GenerateLinkCode();
        string displayLinkCode = (linkCode.Substring(0, 3) + '-' + linkCode.Substring(3, 3) + '-' + linkCode.Substring(6, 3)).ToUpper();
        textBox.text = textTemplate.Replace("{{CODE}}", displayLinkCode);
        StartCoroutine(PollForRefreshToken());
    }

    IEnumerator PollForRefreshToken()
    {
        while (true)
        {
            var task = AuthenticationManager.Instance.FetchPickerSession(linkCode);
            yield return new WaitUntil(() => task.IsCompleted);
            if (task.Result)
            {
                SceneManager.LoadScene("Player");
                break;
            }
            yield return new WaitForSecondsRealtime(5.0f);
        }
    }
}
