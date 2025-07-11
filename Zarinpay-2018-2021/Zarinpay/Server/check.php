<?php
// Developer: Sajad Ahmadi Niat
// At: NasimMehrCreativeIndustries
$order_id = $_GET['order_id'] ?? '';
$mark_delivered = isset($_GET['mark_delivered']) ? true : false;
if (!$order_id) { echo json_encode(['status'=>'error','msg'=>'order_id missing']); exit; }

$db = file_exists('paylogs.json') ? json_decode(file_get_contents('paylogs.json'), true) : [];
if (isset($db[$order_id])) {
    $status = $db[$order_id]['status'];
    if ($mark_delivered && $status === 'paid') {
        $db[$order_id]['status'] = 'delivered';
        file_put_contents('paylogs.json', json_encode($db));
        echo json_encode(['status'=>'paid','ref_id'=>$db[$order_id]['ref_id'] ?? '', 'amount'=>$db[$order_id]['amount'] ?? 0]);
        exit;
    } elseif ($status === 'delivered') {
        echo json_encode(['status'=>'delivered']);
        exit;
    } else {
        echo json_encode(['status'=>$status]);
        exit;
    }
}
echo json_encode(['status'=>'none']);
