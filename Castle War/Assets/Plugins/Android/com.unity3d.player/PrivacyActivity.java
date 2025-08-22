package com.unity3d.player;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.webkit.WebView;
 
public class PrivacyActivity extends Activity implements DialogInterface.OnClickListener {

   // 隐私协议内容
    final String privacyContext = "1. 引言\n"
        + "感谢您使用本游戏（以下简称\"游戏\"）。我们非常重视您的隐私保护，尊重您的个人信息权利。"
        + "为了帮助您了解我们如何收集、使用、存储和保护您的个人信息，本隐私政策详细介绍了我们的信息收集和处理方式。"
        + "请您在使用本游戏前，仔细阅读本隐私政策。\n\n"
        + "2. 我们收集的信息\n"
        + "我们通过 Unity3D SDK 等第三方服务收集以下信息：\n\n"
        + "2.1 设备信息\n"
        + "为了提升用户体验，支持游戏的运行，我们可能会收集以下设备信息：\n\n"
        + "Android ID: 我们通过 Unity3D SDK 获取 Android 设备的唯一标识符 Android ID。"
        + "该信息用于识别设备、分析游戏性能、进行用户行为分析以及提供个性化的广告推荐。\n\n"
        + "传感器信息：我们通过 Unity3D SDK 获取设备的传感器信息(如加速度传感器、陀螺仪等)。"
        + "这些信息有助于增强游戏的交互体验，例如用于虚拟现实(VR)和增强现实(AR)功能、物理模拟等。\n\n"
        + "2.2 数据收集的方式\n"
        + "Unity3D SDK 会在您安装并启动游戏时，自动收集并传输设备信息到我们的服务器。您无需进行任何额外操作。\n\n"
        + "我们还可能使用其他第三方 SDK(例如广告和分析服务)，它们可能会收集您的设备信息。"
        + "具体收集的内容和目的请参见相关第三方的隐私政策。\n\n"
        + "2.3 数据使用的目的\n"
        + "我们收集的 Android ID 和传感器信息主要用于以下目的：\n\n"
        + "设备识别和游戏分析：通过 Android ID, 我们能够唯一地标识您的设备并分析设备在游戏中的使用情况，包括游戏性能、崩溃日志、用户行为等。\n\n"
        + "游戏优化和个性化体验：我们使用传感器信息来优化游戏的物理效果、提升游戏互动体验，例如在虚拟现实(VR)或增强现实(AR)场景中使用。\n\n"
        + "广告和推广：我们可能会利用收集的 Android ID 来展示个性化广告内容，并根据您的设备特征和游戏行为提供相关广告和活动推荐。\n\n"
        + "功能支持：部分游戏功能（如运动感应控制）可能依赖于传感器信息。\n\n"
        + "2.4 数据存储与安全\n"
        + "我们将采取合理的技术措施，确保您的信息安全。收集到的 Android ID 和传感器信息将被加密存储，并仅限于为游戏优化、分析和广告目的使用。\n\n"
        + "2.5 第三方服务\n"
        + "本游戏可能会与第三方公司合作，例如广告提供商、分析服务提供商等。"
        + "这些第三方可能会通过 Unity3D SDK 或其他 SDK 收集和处理您的信息。具体信息请参阅这些第三方的隐私政策。\n\n"
        + "Unity3D SDK 的隐私政策可以参考: Unity隐私政策\n\n"
        + "广告服务：如果您同意展示个性化广告，您的设备信息(包括 Android ID)可能会被广告商收集，以便提供更精准的广告。\n\n"
        + "3. 您的选择\n"
        + "3.1 数据收集设置\n"
        + "您可以在游戏设置中选择是否允许我们收集您的设备信息。禁用某些信息的收集可能会影响游戏的某些功能，或导致您无法获得个性化的游戏体验。\n\n"
        + "3.2 广告设置\n"
        + "您可以通过设备的隐私设置禁用广告跟踪功能，从而减少个性化广告的展示。\n\n"
        + "4. 数据共享\n"
        + "我们不会将您的个人信息出售给第三方，但我们可能会与以下对象共享您的信息：\n\n"
        + "合作伙伴和服务提供商：为了支持游戏的运营和提供相关服务（如广告展示、数据分析等），我们可能将您的信息共享给合作伙伴和服务提供商。\n\n"
        + "法律要求：如果法律要求或为了保护我们、用户或他人的权利，我们可能会披露您的信息。\n\n"
        + "5. 数据存储与保留\n"
        + "我们将根据需要保留您的设备信息，直到不再需要它们来提供服务或遵守法律要求为止。\n\n"
        + "6. 儿童隐私\n"
        + "本游戏不面向儿童(12岁以下)，且我们不会故意收集儿童的个人信息。"
        + "如果您认为我们不小心收集了儿童的信息，请立即与我们联系，我们将删除相关信息。\n\n"
        + "7. 政策变更\n"
        + "我们可能会根据需要更新本隐私政策。当我们更新政策时，我们将通过游戏内弹窗通知您，并在游戏的隐私政策页面上发布更新版本。"
        + "请定期查阅本政策，以确保您了解最新的信息处理方法。\n\n"
        + "8. 联系方式\n"
        + "如果您对本隐私政策有任何疑问或意见，请通过以下方式联系我们：\n\n"
        + "邮箱：[2420638649@qq.com]\n"
        + "电话：[+86 17786084594]\n";

    
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
  
        // 如果已经同意过隐私协议则直接进入Unity Activity
        if (GetPrivacyAccept()){
            EnterUnityActivity();
            return;
        }
        // 弹出隐私协议对话框
        ShowPrivacyDialog();
    }
 
    // 显示隐私协议对话框
    private void ShowPrivacyDialog(){
        WebView webView = new WebView(this);
        webView.loadData(privacyContext, "text/html", "utf-8");         
        AlertDialog.Builder privacyDialog = new AlertDialog.Builder(this);
        privacyDialog.setCancelable(false);
        privacyDialog.setView(webView);
        privacyDialog.setTitle("隐私政策");
        privacyDialog.setNegativeButton("拒绝",this);
        privacyDialog.setPositiveButton("同意",this);
        privacyDialog.create().show();
    }
    
    @Override
    public void onClick(DialogInterface dialogInterface, int i) {
        switch (i){
            case AlertDialog.BUTTON_POSITIVE://点击同意按钮
                SetPrivacyAccept(true);
                EnterUnityActivity(); //启动Unity Activity
                break;
            case AlertDialog.BUTTON_NEGATIVE://点击拒绝按钮,直接退出App
                finish();
                break;
        }
    }
    
    // 启动Unity Activity
    private void EnterUnityActivity(){
        Intent unityAct = new Intent();
        unityAct.setClassName(this, "com.unity3d.player.UnityPlayerActivity");
        this.startActivity(unityAct);
    }
    
    // 本地存储保存同意隐私协议状态
    private void SetPrivacyAccept(boolean accepted){
        SharedPreferences.Editor prefs = this.getSharedPreferences("PlayerPrefs", MODE_PRIVATE).edit();
        prefs.putBoolean("PrivacyAcceptedKey", accepted);
        prefs.apply();
    }
    
    // 获取是否已经同意过
    private boolean GetPrivacyAccept(){
        SharedPreferences prefs = this.getSharedPreferences("PlayerPrefs", MODE_PRIVATE);
        return prefs.getBoolean("PrivacyAcceptedKey", false);
    }
}
