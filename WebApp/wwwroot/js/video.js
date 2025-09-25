export async function uploadVideo(CHUNK_SIZE, userId){
    const file = document.getElementById("fileInput").files[0];
    if (!file) return alert("請選擇影片");

    // UploadWithWebSocket(file, userId);
    // UploadWithHTTP(file, userId, CHUNK_SIZE);
    UploadWithWebworker(file, userId);
}

async function UploadWithHTTP(file, userId, CHUNK_SIZE){
    const fileId = Date.now().toString(); // 簡單用時間戳作 fileId
    const totalChunks = Math.ceil(file.size / CHUNK_SIZE);

    const progressBar = document.getElementById("progressBar");
    const statusText = document.getElementById("status");

    async function uploadChunk(index) {
        const start = index * CHUNK_SIZE;
        const end = Math.min(start + CHUNK_SIZE, file.size);
        const chunk = file.slice(start, end);

        const form = new FormData();
        form.append("chunk", chunk);
        form.append("fileId", fileId);
        form.append("chunkIndex", index);

        await fetch("/Video/Chunk", {
            method: "POST",
            body: form
        });
    }


    // 依序上傳 chunk
    for (let i = 0; i < totalChunks; i++) {
        await uploadChunk(i);
        progressBar.value = ((i + 1) / totalChunks) * 100;
        statusText.innerText = `已上傳 ${i + 1}/${totalChunks} 個分片`;
    }

    

    // 上傳完成後合併
    const mergeForm = new FormData();
    mergeForm.append("fileId", fileId);
    mergeForm.append("filename", file.name);
    mergeForm.append("totalChunks", totalChunks);
    mergeForm.append("userId", userId);

    const res = await fetch("/Video/Merge", {
        method: "POST",
        body: mergeForm
    });
    const data = await res.json();

    if (data.ok) {
        statusText.innerText = "影片上傳完成！";
        fetch(`/User/VideoList?userId=${userId}`)
        .then(res => res.text())
        .then(html => {
            document.getElementById("videoList").innerHTML = html;
        })

    } else {
        statusText.innerText = "上傳失敗：" + (data.error || "未知錯誤");
    }
}

async function UploadWithWebSocket(file, userId){
    const progressBar = document.getElementById("progressBar");
    const statusText = document.getElementById("status");
    
    const socket = new WebSocket("ws://localhost:5021/ws");
    socket.binaryType = "arraybuffer";
    let videoId = file.name;
    
    socket.onopen = async () => {
        const startMsg = {
            action: "START",
            userId: userId,
            fileId: videoId
        };
        socket.send(JSON.stringify(startMsg)); // 通知後端開始檔案

        const CHUNK_SIZE = 1024 * 1024; // 1MB
        const totalChunks = Math.ceil(file.size / CHUNK_SIZE);

        const readChunkAsArrayBuffer = (chunk) => new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onload = () => resolve(reader.result);
            reader.onerror = () => reject(reader.error);
            reader.readAsArrayBuffer(chunk);
        });

        for (let i = 0; i < totalChunks; i++) {
            const start = i * CHUNK_SIZE;
            const end = Math.min(start + CHUNK_SIZE, file.size);
            const chunk = file.slice(start, end);

            const buffer = await readChunkAsArrayBuffer(chunk);
            socket.send(buffer); // 上傳 chunk

            progressBar.value = ((i + 1) / totalChunks) * 100;
            statusText.innerText = `已上傳 ${i + 1}/${totalChunks} 個分片`;
        }
        const endMsg = {
            action: "END",
        };
        socket.send(JSON.stringify(endMsg)); // 上傳完成通知後端
        socket.close(); // 再關閉連線
    };

    socket.onmessage = (event) => {
        videoId = event.data;
        console.log(videoId);
    };

    socket.onclose = async () => {
        fetch(`/Video/CheckVideo?videoId=${videoId}&userId=${userId}`)
        .then(result => result.json())
        .then(data => {
            if(data.success){
                statusText.innerText = "影片上傳完成！";
                fetch(`/User/VideoList?userId=${userId}`)
                .then(res => res.text())
                .then(html => {
                    document.getElementById("videoList").innerHTML = html;
                })
            }
        })
    };
}

async function UploadWithWebworker(file, userId){
    const progressBar = document.getElementById("progressBar");
    const statusText = document.getElementById("status");

    const WORKER_COUNT = 4;  // 開幾個 worker
    const CHUNK_SIZE = 1024 * 1024; // 1MB
    const totalChunks = Math.ceil(file.size / CHUNK_SIZE);

    const worker = new Worker("/js/upload-worker.js");
    let completedChunks = 0;
    let videoId = file.name;

    const wsProtocol = location.protocol === "https:" ? "wss" : "ws";
    const wsHost = location.hostname;
    const wsPort = location.port || (location.protocol === "https:" ? 443 : 80);
    const wsUrl = `${wsProtocol}://${wsHost}:${wsPort}/ws`;

    worker.postMessage({
        file,
        CHUNK_SIZE: CHUNK_SIZE,
        userId: userId,
        wsUrl: wsUrl
    });

    worker.onmessage = (e) => {
        if (e.data.type === "progress") {
            completedChunks++;
            progressBar.value = (completedChunks / totalChunks) * 100;
            statusText.innerText = `已上傳 ${completedChunks}/${totalChunks} 個分片`;
            if(completedChunks == totalChunks){
                fetch(`/Video/CheckVideo?videoId=${videoId}&userId=${userId}`)
                .then(result => result.json())
                .then(data => {
                    if(data.success){
                        statusText.innerText = "影片上傳完成！";
                        fetch(`/User/VideoList?userId=${userId}`)
                        .then(res => res.text())
                        .then(html => {
                            document.getElementById("videoList").innerHTML = html;
                        })
                    }
                })
            }
        }else if(e.data.type === "Rename"){
            videoId = e.data.name;
        }
    };

}

export async function DeleteVideo(fileName, userId){
    const res = await fetch(`/File/DeleteVideo?fileName=${fileName}&userId=${userId}`);
    const data = await res.json();
    if (data.ok) {
        fetch(`/User/VideoList?userId=${userId}`)
        .then(res => res.text())
        .then(html => {
            document.getElementById("videoList").innerHTML = html;
        })
    }
}

export async function Record(userId, videoId, currentTime){
    const form = new FormData();
    form.append("userId", userId);
    form.append("videoId", videoId);
    form.append("lp", Math.floor(currentTime));
    
    await fetch("/Video/Record", {
        method: "POST",
        body: form
    });
}

export async function loadLastPlayback(userId, videoId) {            
    const res = await fetch(`/Video/GetLog?userId=${userId}&videoId=${videoId}`);
    const data = await res.json();
    if (data.lastPosition && data.lastPosition > 0) {
        return data.lastPosition;
    }else{
        return 0;
    }
}

export function hmsToSeconds(h, m, s) {
    return h*3600 + m*60 + s;
}

export function SecondsTohms(seconds){
    seconds = Number(seconds);
    const h = Math.floor(seconds / 3600);
    const m = Math.floor((seconds % 3600) / 60);
    const s = Math.floor(seconds % 60);
    return { h, m, s };
}
