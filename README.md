# eeg-control-aircraft

> Use EmotivEPOC Neuroheadset to control aircraft

> 使用EmotivEPOC耳机采集EEG数据通过滤波、分类、对比后控制四轴飞行器

###System Architecture

Brain wave data using headphones get transmitted via Bluetooth adapter to the PC computer, the computer brain wave data collected to use MATLAB for filtering the data (only with DB4 wavelet transform filtering), filtered through software for data classification and comparison (software has learning the wearer's brainwaves data) collected the wearer's brainwaves signal is then transmitted via a Bluetooth adapter to the aircraft axis, so that the aircraft can be controlled axis.

![](http://i12.tietuku.com/c9fa1b98aa54a7f2.jpg)

###Software Operation

Correct wear EmotivEPOC Neuroheadset(16 channels are displayed green), correctly open software and aircraft, check for the USB adapter and the flight vehicle adapter for the headset.

1.Check headset is worn correctly

![](http://i5.tietuku.com/1f4cb5551b158c4d.png)

2.Brain wave study

![](http://i12.tietuku.com/a52ec26a5a9b0588.png)

3.EEG control after training

![](http://i12.tietuku.com/ea11e85ff872bcaf.png)

4.PC control the aircrafe

![](http://i12.tietuku.com/9b2c817a31877749.png)

5.Brain waves control the aircrafe

![](http://i12.tietuku.com/c76483f6215f0f39.png)

###Production Description

We used Emotiv Epoc headphone producted by Emotiv Systems Company. They provide the C# api  so that we can collect headset data directly. Please see [emotiv website](http://www.emotiv.com)

---

Author:[CMB](http://chenmingbiao.github.io)




