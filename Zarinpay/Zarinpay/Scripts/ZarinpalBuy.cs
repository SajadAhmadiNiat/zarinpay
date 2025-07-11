using System;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Developer: Sajad Ahmadi Niat
// At: NasimMehrCreativeIndustries

public class ZarinpalBuy : MonoBehaviour
{
    [Header("PayData")]
    string merchant_id = "XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX";
    string orderId = "";
    int _lastAmountChecked = 0;

    [Header("GameData")]
    ShopProductType _productType;
    public Text Coin;
    public Text StatusText;
    int _coin = 0;
    public GameObject _Successfull_panel;
    public GameObject _Paying_panel;
    public GameObject _Failed_panel;
    public GameObject _Unspecified_panel;
    public GameObject _NoConnect_panel;
    public GameObject _rec_panel;

    public enum ShopProductType
    {
        Coin1 = 1,
        Coin5 = 2,
    }
    public void StartPaymentByProduct(int type)
    {
        _productType = (ShopProductType)type;
        int payAmount = 0;
        StatusText.color = Color.white;
        StatusText.text = "درحال آماده سازی ...";

        switch (_productType)
        {
            case ShopProductType.Coin1:
                payAmount = 10000;
                break;
            case ShopProductType.Coin5:
                payAmount = 30000;
                break;
        }

        orderId = Guid.NewGuid().ToString();
        _lastAmountChecked = payAmount;
        string callback_url = $"https://yourdomain.ir/verify.php?order_id={orderId}&amount={payAmount}";
        StartCoroutine(RequestPayment(callback_url, payAmount));
    }
    IEnumerator RequestPayment(string callbackUrl, int pAmount)
    {
        StatusText.color = Color.white;
        StatusText.text = "ورود به درگاه پرداخت ...";
        _Paying_panel.SetActive(true);

        var obj = new PaymentReq()
        {
            merchant_id = merchant_id,
            amount = pAmount,
            callback_url = callbackUrl,
            description = "خرید از فروشگاه بازی"
        };
        string postJson = JsonUtility.ToJson(obj);

        using (UnityWebRequest req = new UnityWebRequest("https://api.zarinpal.com/pg/v4/payment/request.json", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(postJson);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                var resp = JsonUtility.FromJson<PaymentResp>(req.downloadHandler.text);
                if (resp != null && resp.data != null && resp.data.code == 100)
                {
                    string authority = resp.data.authority;
                    string payUrl = "https://www.zarinpal.com/pg/StartPay/" + authority;
                    Application.OpenURL(payUrl);
                }
                else
                {
                    Debug.LogError("Error in payment request: " + req.downloadHandler.text);
                    StatusText.color = Color.red;
                    StatusText.text = $"خطا در درخواست پرداخت:{req.downloadHandler.text}";
                    _Failed_panel.SetActive(true);
                    _Paying_panel.SetActive(false);
                }
            }
            else
            {
                Debug.LogError("Error sending request: " + req.error);
                StatusText.color = Color.red;
                StatusText.text = $"خطا در ارسال درخواست:{req.error}";
                _Failed_panel.SetActive(true);
                _Paying_panel.SetActive(false);
            }
        }
    }
    public void CheckPaymentStatus()
    {
        StartCoroutine(CheckPaymentCoroutine());
    }
    IEnumerator CheckPaymentCoroutine()
    {
        string url = $"https://yourdomain.ir/check.php?order_id={orderId}&mark_delivered=1";
        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            var json = req.downloadHandler.text;
            var resp = JsonUtility.FromJson<PaymentStatusResp>(json);

            if (resp.status == "paid")
            {
                Debug.Log("Successful!");
                StatusText.color = Color.green;
                StatusText.text = "پرداخت موفق!";
                switch (_productType)
                {
                    case ShopProductType.Coin1:
                        _coin += 1;
                        Coin.text = _coin.ToString();
                        Debug.Log("Added 1 Coin");
                        break;
                    case ShopProductType.Coin5:
                        _coin += 5;
                        Coin.text = _coin.ToString();
                        Debug.Log("Added 5 Coin");
                        break;
                }
                _Successfull_panel.SetActive(true);
                _Paying_panel.SetActive(false);
            }
            else if (resp.status == "delivered")
            {
                Debug.Log("payment is recurring.");
                StatusText.color = Color.magenta;
                StatusText.text = "تراکنش تکراری، این سفارش قبلا تحویل داده شده";
                _rec_panel.SetActive(true);
                _Paying_panel.SetActive(false);
            }
            else if (resp.status == "failed" || resp.status == "canceled")
            {
                Debug.LogWarning("The payment failed or was canceled by the user.");
                StatusText.color = Color.red;
                StatusText.text = "پرداخت ناموفق بوده یا توسط کاربر کنسل شده است.";
                _Failed_panel.SetActive(true);
                _Paying_panel.SetActive(false);
            }
            else
            {
                Debug.Log("Payment status unclear or not yet completed");
                StatusText.color = Color.yellow;
                StatusText.text = "وضعیت پرداخت نامشخص یا هنوز تکمیل نشده";
                _Failed_panel.SetActive(true);
                _Paying_panel.SetActive(false);
            }
        }
        else
        {
            Debug.LogError("Error communicating with the payment status server.");
            StatusText.color = Color.red;
            StatusText.text = "خطا در ارتباط با سرور وضعیت پرداخت";
            _NoConnect_panel.SetActive(true);
            _Paying_panel.SetActive(false);
        }
    }
    [Serializable] public class PaymentReq { public string merchant_id; public int amount; public string callback_url; public string description; }
    [Serializable] public class PaymentResp { public PaymentData data; }
    [Serializable] public class PaymentData { public int code; public string authority; }
    [Serializable] public class PaymentStatusResp { public string status; public string ref_id; }
}
