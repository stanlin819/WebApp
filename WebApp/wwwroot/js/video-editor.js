import * as v from "./video.js";

const { createFFmpeg, fetchFile } = FFmpeg;
const ffmpeg = createFFmpeg({ log: true });

const video = document.getElementById("video-player");

const fileName = video.dataset.filename;
const userId = video.dataset.userid;
const filePath = video.dataset.filepath;


const filterList = document.getElementById('filters');
const resetBtn = document.getElementById("resetBtn");

let videoLength;
let fileData;

video.addEventListener('loadedmetadata', async () => {
    videoLength = video.duration;
    const { h, m, s } = v.SecondsTohms(videoLength);

    const endHour = document.getElementById('endHour');
    const endMinute = document.getElementById('endMinute');
    const endSecond = document.getElementById('endSecond');

    endHour.value = h;
    endMinute.value = m;
    endSecond.value = s;
    video.currentTime = await v.loadLastPlayback(userId, fileName);
    setInterval(async ()=>{
        await v.Record(userId, fileName, video.currentTime);
    }, 5000);
});

async function loadVideo(){
    const response = await fetch(filePath); // 伺服器 URL
    if (response.ok){
        const blob = await response.blob();
        fileData = new File([blob], fileName, { type: blob.type });
        video.src = URL.createObjectURL(fileData);
    }
}

const updateFilters = function () {
    const items = [...filterList.children];
    const filterStrings = items.map(item => {
        const filterName = item.dataset.filter;
        const value = item.querySelector('input').value;
        switch (filterName) {
        case 'grayscale':
            return `${filterName}(${value}%)`;
        case 'brightness':
            return `${filterName}(${value}%)`;
        case 'blur':
            return `${filterName}(${value}px)`;
        }
    });
    video.style.filter = filterStrings.join(' ');
}

filterList.querySelectorAll('input').forEach((input) => {
    input.addEventListener('input', updateFilters);
});

resetBtn.addEventListener('click', e => {
    const items = [...filterList.children];
    items.forEach(item => {
        const input = item.querySelector('input');
        const filterName = item.dataset.filter;
        switch (filterName) {
        case 'grayscale':
            input.value = 0;
            break;
        case 'brightness':
            input.value = 100;
            break;
        case 'blur':
            input.value = 0;
            break;
        }
    });

    video.style.filter = '';

    updateFilters();
});

//剪輯
trimBtn.addEventListener('click', async () => {
    if (!fileData) {
        alert('Please select a video first');
        return;
    }

    const startTime = v.hmsToSeconds(
        parseInt(document.getElementById('startHour').value),
        parseInt(document.getElementById('startMinute').value),
        parseInt(document.getElementById('startSecond').value)
    );

    // 取得結束時間秒數
    const endTime = v.hmsToSeconds(
        parseInt(document.getElementById('endHour').value),
        parseInt(document.getElementById('endMinute').value),
        parseInt(document.getElementById('endSecond').value)
    );

    const start = parseFloat(startTime);
    const end = parseFloat(endTime);

    if (start >= end || start < 0 || end > videoLength) {
        alert('Incorrect time range');
        return;
    }

    // 載入 ffmpeg.wasm
    if (!ffmpeg.isLoaded()) {
        await ffmpeg.load();
    }

    // 將影片寫入虛擬檔案系統
    ffmpeg.FS('writeFile', fileData.name, await fetchFile(fileData));
    
    const items = [...filterList.children];
    const filterStrings = items.map(item => {
        const filterName = item.dataset.filter;
        const value = item.querySelector('input').value;
        switch (filterName) {
        case 'grayscale':
            const gray = Math.abs(value / 100 - 1);
            return `hue=s=${gray}`;
        case 'brightness':
            const bright = value / 100;
            return `eq=brightness=${(bright-1) / 2.5}`;
        case 'blur':
            return `boxblur = ${value}:2`;
        }
    });
    const vfFilters = filterStrings.join(',');

    let format = fileData.name.endsWith('.mp4') ? 'mp4':'webm';

    // 執行剪輯
    await ffmpeg.run(
        '-i', fileData.name,
        '-ss', `${start}`,
        '-to', `${end}`,
        '-vf', vfFilters,
        `output.${format}`
    );

    // 讀取輸出檔案
    const data = ffmpeg.FS('readFile', `output.${format}`);
    const blob = new Blob([data], { type: `video/${format}` });
    const url = URL.createObjectURL(blob);
    const name = fileData.name;
    previewVideo.src = url;
    previewVideo.style.display = 'block';

    downloadLink.href = url;
    downloadLink.download = name.replace(`.${format}`, `_clip.${format}`);
    downloadLink.style.display = 'inline';
    downloadLink.textContent = 'Download video';
});

//轉碼
transBtn.addEventListener('click', async () => {
    if (!fileData) {
        alert('Please select a video first');
        return;
    }
    
    const name = fileData.name;
    let outputFile;

    const format = name.endsWith('.mp4') ? 'webm':'mp4';
    // 載入 ffmpeg.wasm
    if (!ffmpeg.isLoaded()) {
        await ffmpeg.load();
    }

    // 將影片寫入虛擬檔案系統
    ffmpeg.FS('writeFile', 'input', await fetchFile(fileData));

    if(format === 'webm') {
        // MP4 → WebM
        await ffmpeg.run(
        '-i', 'input',
        '-c:v', 'libvpx-vp9', // 視訊編碼器：VP9
        '-preset', 'fast',
        '-b:v', '1M', // 視訊位元率：1Mbps
        '-c:a', 'libopus', // 音訊編碼器：Opus
        name.replace(`.mp4`, `.webm`)
        );
        outputFile = name.replace(`.mp4`, `.webm`);
    } else {
        // WebM → MP4
        await ffmpeg.run(
        '-i', 'input',
        '-c:v', 'libx264', // 視訊編碼器：H.264
        '-crf', '23', // 品質控制參數 (23 為預設，數值越小畫質越高檔案越大)
        '-preset', 'fast', // 編碼速度 (fast=快速, 檔案稍大; slower=更高壓縮率)
        '-c:a', 'aac', // 音訊編碼器：AAC
        '-b:a', '128k', // 音訊位元率：128kbps
        name.replace(`.webm`, `.mp4`)
        );
        outputFile = name.replace(`.webm`, `.mp4`);
    }

    // 讀取輸出檔案並生成下載連結
    const data = ffmpeg.FS('readFile', outputFile);
    const url = URL.createObjectURL(new Blob([data.buffer], { type: `video/${format}` }));
    const downloadLink = document.getElementById('downloadLink');
    downloadLink.href = url;
    downloadLink.download = outputFile;
    downloadLink.style.display = 'inline';
});

loadVideo();

updateFilters();