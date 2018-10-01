Overview
--------

What does it currently do
_________________________

- This project aims to be the first lightweight API server for NEO, which can be installed and used right away without requiring  synchronizing with the NEO Production Blockchain. If you have any experience using neo-python, you'll understand the benefit of not having to synchronize.
- All transactions can be securely processed without having open wallets on the server. All of the other NEO server apps require an existing wallet to remain noopen.
- Each operation rotates to a different RPC node. All of the other NEO apps stick to the most recent working RPC node.
- RPC Nodes are checked before performing operations them. Slow, dead or otherwise unreachable nodes are discarded, so, the server is always connected to working fast RPC nodes.
- Gracefully handles failed transactions by using up to configurable retries spread out over a configurable timespan. 
.- Ability to close the server to ensure selected clients can use it, which gives us control for specific products.
- Easy to use, JSON focused API. 

What it will do
______________

- Check heights
- Logged in users
- Monitoring UI

