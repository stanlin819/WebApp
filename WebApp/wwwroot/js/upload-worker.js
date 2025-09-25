self.onmessage = async (e) => {
    const {file, CHUNK_SIZE, userId, wsUrl} = e.data;


    const socket = new WebSocket(wsUrl);
    socket.binaryType = "arraybuffer";
    let videoId = file.name;
    
    socket.onopen = async () => {
        const startMsg = {
            action: "START",
            userId: userId,
            fileId: videoId
        };
        socket.send(JSON.stringify(startMsg)); // 通知後端開始檔案

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

            self.postMessage({ type: "progress"});
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
        self.postMessage({type: "Rename", name: videoId});
    };
};