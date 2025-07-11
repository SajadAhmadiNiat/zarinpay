<?php
// Developer: Sajad Ahmadi Niat
// At: NasimMehrCreativeIndustries
$merchant_id = 'XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX';
$amount = isset($_GET['amount']) ? intval($_GET['amount']) : 0;
$status = $_GET['Status'] ?? '';
$authority = $_GET['Authority'] ?? '';
$order_id = $_GET['order_id'] ?? '';
$result_msg = '';
$is_paid = false;

if ($status == 'OK' && $authority) {
    $params = [
        "merchant_id" => $merchant_id,
        "authority" => $authority,
        "amount" => $amount
    ];
    $ch = curl_init('https://api.zarinpal.com/pg/v4/payment/verify.json');
    curl_setopt($ch, CURLOPT_POSTFIELDS, json_encode($params));
    curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
    curl_setopt($ch, CURLOPT_HTTPHEADER, ["Content-Type: application/json"]);
    $res = curl_exec($ch);
    curl_close($ch);

    $out = json_decode($res, true);

    if (isset($out['data']) && $out['data']['code'] == 100) {
        $result_msg = '✅ پرداخت با موفقیت تایید شد!';
        $is_paid = true;

        $db = file_exists('paylogs.json') ? json_decode(file_get_contents('paylogs.json'), true) : [];
        $db[$order_id] = [
            'authority' => $authority,
            'ref_id' => $out['data']['ref_id'],
            'status' => 'paid',
            'amount' => $amount
        ];
        file_put_contents('paylogs.json', json_encode($db));
    } else {
        $result_msg = '❌ پرداخت تایید نشد! خطا: ' . ($out['data']['message'] ?? 'خطای نامشخص');
        if ($order_id) {
            $db = file_exists('paylogs.json') ? json_decode(file_get_contents('paylogs.json'), true) : [];
            $db[$order_id] = [
                'authority' => $authority,
                'status' => 'failed',
                'amount' => $amount
            ];
            file_put_contents('paylogs.json', json_encode($db));
        }
    }
} else {
    $result_msg = '❌ پرداخت لغو یا نامعتبر!';
    if ($order_id) {
        $db = file_exists('paylogs.json') ? json_decode(file_get_contents('paylogs.json'), true) : [];
        $db[$order_id] = [
            'authority' => $authority,
            'status' => 'canceled',
            'amount' => $amount
        ];
        file_put_contents('paylogs.json', json_encode($db));
    }
}
?>
<html>
    <body style="font-family:tahoma;text-align:center;direction:rtl;margin-top:80px;">
        <h1><?php echo $result_msg; ?></h1>
        <p>حالا به برنامه برگردید و دکمه "بررسی پرداخت" را بزنید.</p>
    </body>
</html>
