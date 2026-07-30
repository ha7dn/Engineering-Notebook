### Why is voice chat so hard to get right?
#### Scalability in chat apps
- They have to be able to support a large number of users without affecting performance or reliability.
- Has to be able to handle increased traffic, keep low latency and guarantee unhindered communications
- Has to provide a way to introduce new features with the least disruption in its user experience

#### The traditional approach: Client A ➡ Central Server ➡ Client B
- Problems:
	- Single point of failure
	- Latency increases with distance from the server
	- Bandwidth costs scale up with participants
	- Quality limited by the weakest connection
	- No redundancy or failover
	- Every network jump increases the chances of packet loss

####  Layer 1: The Intelligent Gateway System
Instead of routing all voice through central servers, Discord uses a global network of _voice gateways_ that act like intelligent switches.
##### The Gateway Selection Algorithm
Gateway Selection Process:
1. Measure latency to all available gateways
2. Check current gateway load and capacity
3. Analyse participant geographic distribution
4. Factor in ISP routing preferences
5. Consider current internet weather conditions
6. Select optimal gateway with backup failover

⚠ If network conditions change during the call, Discord migrates the connection to a better gateway without dropping the audio.

####  Layer 2: Adaptive Audio Processing
Discord's system adapts its processing pipeline based on real time analysis of the participants' voice, network and hardware.
##### The Dynamic Audio Pipeline
Per-user Input Audio Processing:
- **Voice Activity Detection**
	- Silence suppression ([saves 60-80% bandwidth])
	- Background noise filtering
	- Echo cancellation tuning
	- Dynamic range adjustment
-  **Codec selection (in real time)**
	- Opus for high quality connections
	- Opus with lower [[bitrate]] for medium connections
	- Legacy codecs for poor connections
	- Dynamic switching based on network conditions
- **Packet prioritization**
	- Voice packets get highest priority
	- Redundant data for critical audio segments
	- Smart buffering for network hiccups
	- Adaptive [jitter buffering](obsidian://open?vault=Code&file=_Study%2F_Concepts%2FJitter%20buffer)

⚠ Discord analyses the audio content. Speaking voice is treated differently than music, singing or normal conversation. Gaming callouts are prioritized over casual chat. The system learns your voice patterns and optimizes accordingly.

####  Layer 3: The Mesh Network Protocol
##### The Hybrid Mesh System
Connection Topology (Dynamic):
- **Low Latency Mode**
	- Direct P2P for participants with good connections
	- Server relay [only] when P2P fails
	- Automatic fallback and recovery
	- Real time quality monitoring
- **High Reliability Mode**
	- Server mediated for stable connections
	- Multiple redundant paths
	- Aggressive error correction
	- Quality over latency optimization
- **Adaptive Mode (default)**
	- Mix of P2P and server connections
	- Real time switching based on conditions
	- Predictive connection establishment
	- Load balancing across available paths

**EXAMPLE:** Imagine you're in a Discord voice channel with 5 people:
- 2 of them have excellent connections, so they might connect directly to each other
- 3 of them with moderate connections route through the optimal gateway
- If someone's connection degrades, they automatically fall back to server relay
- If a gateway overloads, some connections will switch to P2P or alternate gateways
- All of this [without dropping audio]

####  Layer 4: Predictive Quality Management
##### The Prediction Engine
Network Condition Prediction:
- Historical performance data for your connection
- ISP routing table analysis
- Time-of-day congestion patterns
- Geographic internet weather
- Device performance profiling
- Real time bandwidth monitoring

####  The Protocol Stack
##### Discord's Voice Protocol optimized for real-time voice communication
- Application Layer
	- Voice Activity Detection
	- Audio Processing Pipeline
	- Quality Adaptation Logic
	- User Experience Management
- Transport Layer
	- Custom [[UDP]] with reliability extensions
	- Automatic repeat request (ARQ) for critical packets
	- Forward Error Correction
	- Adaptive Congestion Control
- Network Layer
	- Multi-path routing
	- Gateway selection algorithm
	- Load Balancing Logic
	- Failover Management
- Physical Layer
	- Network condition monitoring
	- Bandwidth estimation
	- Jitter measurement
	- Packet loss detection

#### The Real-Time Adaptation System
##### Quality Adaptation Decision Tree (Every 100ms)
- Check network conditions
	- Packet loss > 5%?
		- ✔ → Reduce bitrate, increase Forward Error Correction
		- ❌ → Continue
	- Latency > 150ms?
		- ✔ → Switch to lower latency codec/gateway
		- ❌ → Continue
	- Jitter > 30ms?
		- ✔ → Increase buffer size
		- ❌ → Continue
	- More bandwidth required?
		- ✔ → Enable aggressive compression
		- ❌ → Optimize for quality
	- Gateway overloaded?
		- ✔ → Migrate to backup gateway
		- ❌ → Continue

### Why Discord outperforms the competition
#### Zoom
It was built for corporate conference rooms with reliable internet and predictable usage patterns.

**Architecture problems:**
- Centralized processing creates bottlenecks
- Fixed quality settings don't adapt to conditions
- No predictive optimization
- Limited global infrastructure
- Enterprise focus misses consumer use cases
- Legacy architecture can't handle modern scale

#### Microsoft Teams
They prioritized security, compliance and integration over user experience and performance

**Architecture problems:**
- Security overhead adds latency
- Integration complexity reduces reliability
- Enterprise features consume resources
- Legacy compatibility limits optimization
- Corporate network assumptions fail for home users

#### Google Meet
Meet was architected as a lightweight video calling add-on to the G-Suite, not as a primary communication platform

**Architecture problems:**
- Designed for short meetings, not continuous and variable communication
- Limited audio processing capabilities
- No adaptive quality management
- Consumer features added as an afterthought
- Optimization for video over audio
#### Gaming-first requirements that benefit everyone:
- Ultra low latency
- High reliability
- Background noise handling
- Multiple simultaneous audio sources
- Cross-platform compatibility
- Community-driven feature development
#### Discord's optimization focus:
- Long-term connection stability
- Gradual quality improvements over time
- Battery efficiency for mobile users
- Bandwidth optimization for extended use
- Social features that enhance voice communication
#### Discord's Innovations:
##### The Perceptual Audio Codec
They've created a perceptual audio processing system that understands the difference between human speech and other audio content
- **Features:**
	- Speech optimization
		- Vocal frequency enhancement
		- Consonant clarity improvement
		- Background voice separation
		- Dynamic range compression for voices
	- Music/Game Audio optimization
		- Full frequency range preservation
		- Stereo imaging maintenance
		- Dynamic range preservation
		- Low-latency processing
##### Machine Learning Network Optimization
Discord uses ML models to predict optimal network configurations
- **ML Applications:**
	- Gateway selection prediction
	- Bandwidth requirement forecasting
	- Connection quality scoring
	- Optimal codec selection
	- Buffer size optimization
	- Failure prediction and prevention
##### The Social Graph Audio Optimizer
Frequent conversation partners get prioritized processing and the system learns your communication patterns.
- **Social Optimization Features:**
	- Priority audio processing
	- Learned noise profiles for regular participants
	- Customized audio enhancements per relationship
	- Group conversation flow optimization

### Discord Architecture Requirements
#### Functional
- Guild-based text channels with categories
- Low-latency voice and video channels
- Screen sharing and Go Live streaming
- Rich presence and activity status
- Bot framework and application commands
- Role-based permissions at guild, category, and channel level
- Direct messages and group DMs
#### Non functional
- Ultra-low voice latency (<50ms codec-to-ear)
- High availability for voice infrastructure
- Horizontal scaling to millions of concurrent guilds
- Sub-second message delivery globally
- Support for guilds with 1M+ members
- Efficient storage for trillions of messages
### Discord Architecture Overview

![[Pasted image 20260715183930.png]]
- **🟪 Elixir/BEAM.** Core real-time backend. Around 20 microservices using Erlang with partial mesh topology for service discovery.
- **🟧 Rust.** Performance-critical services: Read states, Data services (DB Proxy), Media Proxy, Game SDK, Go Live video capture and [[Elixir NIFs]].
- **🟨Python.** Powers the HTTP Rest API monolith handling CRUD operations for guilds, channels, users and messages.
- **🟦C++.** Voice/Video [[SFU]] media engine, native client audio engine and ScyllaDB itself. Custom engine bypasses OS audio ducking.
#### 1. Client Applications
- Desktop apps
- Mobile apps
- Web Interface
#### 2. Gateway
- It's responsible for managing communication between the client apps and the servers
- It handles authentication, encryption and routing
- Support various protocols ([[Websocket]], HTTP/2, [[gRPC]])
##### 2.1 Websocket Gateway
- Every active cliente maintains a persistent WebSocket connection.
- The gateway pushes messages, presence updates and typing indicators without polling, using [[GenStage]] for back-pressure and load shedding.
- A migration from zlib to Zstandard achieved a 40% reduction in bandwidth usage across all WebSocket connections.![[Pasted image 20260716130108.png|675]]
#### 3. API Servers
- Provides a set of APIs that allows developers to integrate with the platform
- They handle various tasks, like user auth, message management, voice chat, etc
- They are consumed by client apps and 3rd party integrations.
##### 3.1 Bot & API Servers
- Discord exposes 2 API surfaces: the HTTP REST API and the WebSocket Gateway. Bots can receive interactions via persistent gateway connection or outgoing webhooks to a configured URL, enabling serverless architectures.![[Pasted image 20260730142335.png]]
#### 4. Presence and Voice Servers
- They handle user status updates (online, idle, offline)
- Voice servers manage real-time voice communication between users during voice calls and group chats
- They ensure low latency, high quality voice transmission using [[WebRTC]]
##### 4.1 Voice Architecture
- Three backend services power voice: the Discord Gateway (WebSocket events), Discord Guilds (voice server assignment and state), and Discord Voice (signaling and Selective Forwarding Units or [[SFU]]).
- Discord replaces the standard [[Session Description Protocol]] (SDP) signaling of 10KB with a minimal 1000 byte payload containing only server address, encryption method, codec and stream ID. 
- Interactive Connectivity Establishment (ICE) is skipped since all clients connect through relay servers, which also hides user IPs.
- [[Salsa20]] encryption is used for performance.
- During silent periods, no audio is transmitted, requiring sequence number rewriting.
- E2E Encryption is enforced for DMs, group DMs, voice channels and Go Live streams and all non-stage voice calls.
- WebRTC Encoded Transforms + Messaging Layer Security (MLS) for group key exchange with epoch-based rotation is used when participants join or leave. The protocol is open-source and externally audited.![[Pasted image 20260730142103.png]]
#### 5. Data Storage
- Employs distributed databases to store user data, messages, media files and other content
- It uses a combination of SQL and No SQL databases for different types of data
- The systems are designed for scalability, reliability, and low latency access.
##### 5.1 Message Storage
- It evolved from MongoDB to Cassandra to [[ScyllaDB]].
- The [[Rust Data Services]] layer sits between the API and the database to provide [[request coalescing]] and consistent hash routing for [[cache locality]]
- Mesages are partitioend by `channel_id` combined with static time buckets. Each message uses a Snowflake ID and is replicated across 3 nodes.

| Metric               | Cassandra | ScyllaDB     |
| -------------------- | --------- | ------------ |
| [[p99]] Read Latency | 40-125ms  | 15ms         |
| p99 Write Latency    | 5-70ms    | 5ms (steady) |
| Custer size          | 177 nodes | 72 nodes     |
| Disk per node        | -         | 9TB          |
![[Pasted image 20260730134258.png]]
#### 6. [[CDNs]]
- They efficiently distribute static content to users
- They help reduce latency and improve content delivery performance by caching
##### 6.1 CDN & Media Pipeline
- Media is served through two domains: 
	- `cdn.discordap.com` for static originals
	- `media.discordapp.net` for the Rust Media Proxy that inspects, converts and resizes every attachment and embedded image on the fly.
- The open-source library Lilliput handles image processing with WebP, AVIF and GIF support.
- The `is_animated` flag is propagated throughout all API systems and respects the user's Reduced Motion accessibility setting, ensured content can be paused for users who need it. ![[Pasted image 20260730142734.png]]
#### 7. Microservices Architecture
- The backend is composed by numerous microservices
- They communicate with each other via APIs or message queues, allowing for independent development, scaling and fault isolation.
#### 8. Load Balancers and Autoscaling
- They distribute  incoming trafic across multiple servers to ensure optimal performance and reliability.
- [[Autoscaling]] is used to dynamically adjust server capacity based on traffic demand, ensuring scalability and cost efficiency
#### 9. Security
- It employs various security measures, including encryption, authentication,[[ rate limiting ]]and DDoS protection.
- They regularly perform sec audits, pentesting and code reviews to identify and mitigate potential vulnerabilities.
#### 10. Monitoring and Analytics
- They monitor the health and performance of its infrastructure using monitoring tools and analytics platforms.
- They collect and analyse metrics, logs and user feedback to identify issues, optimize performance and improve the user experience.
#### 11. Guild Sharding
- Each Discord server (guild) is represented by a stateful [[GenServer]] process distributed across the cluster using a [[hash ring]].
- Guilds are the atomic unit and cannot be further partitioned.
- BEAM supervision handles process crashes and node failures.
- Socket connections are held by processes, so moving guild processes between nodes means disconnecting users.

| Parameter        | Value                           | Notes                                           |
| ---------------- | ------------------------------- | ----------------------------------------------- |
| Shard formula    | `(guild_id >> 22) % num_shards` | Derives from [[Snowflake ID]] structure         |
| Bot shard limit  | 2500 guilds per shard           | Enforced by Discord                             |
| Recommended      | 1000 guilds per shard           | For optimal performance                         |
| Connection model | 1 WebSocket per shard           | Each shard maintains its own gateway connection |

![[Pasted image 20260730123609.png]]
#### 12. Push Notification Architecture
- The Push Collector (1 process per machine) buffers requests, while the Pusher consumers demand exactly 10 at a time.
- Firebase [[XMPP]] is used instead of HTTP because it enforces a 100-pending-request limit per connection.
- The system handles bursts of +1M push requests per minute via load-shedding when the buffer fills.
#### 13. Elixir + [[Rust NIFs]]
- Guilds with +100K members needed sorted member lists. updating a list when a member joins requires a sorted insertion that reports the index. Pure Elixir solutions topped at 27K μs worst case for 250K items.
- The Rust SortedSet NIFs handles 1M items with sub-4 μs worst case latency. All operations stay under 1ms, eliminating the need for BEAM reductions or yielding. The NIF module now powers every single Discord guild's member list.

| Solution              | Language | 250K best | 250K worst |
| --------------------- | -------- | --------- | ---------- |
| [[MapSet]]            | 🟪Elixir | 31,644μs  | 57,580μs   |
| :[[ordsets]]          | 🟪Elixir | 20,430μs  | 27,390μs   |
| Rust SortedSet (250K) | 🟧Rust   | 0.4μs     | 1.2μs      |
| Rust SortedSet (1M)   | 🟧Rust   | 0.61μs    | 3.68μs     |
#### 14. Read States
- They track which channels and messages each user has read. It's accessed on every connection, message send and read action.
- Go's garbage collector ran every 2 minutes, scanning the [[LRU cache]] checking for unreferenced memory. This caused periodic latency spikes proportional to cache size. Reducing cache size lowered the spike's magnitude, but increased cache misses.
- Rust's ownership-based memory model means evicted items are immediately freed without GC scanning. Average response time dropped to milliseconds, capacity increased to 8M Read States per node and all latency spikes were eliminated.
- It's built on [[Tokio async runtime]] with [[BTreeMap]] for memory efficiency. ![[Pasted image 20260730131015.png]]
#### 15. Message Search and Indexing
- It evolved from 2 large ElasticSearch clusters to a modern "cell" topolgy of 40 smaller clusters on K8s with [[ECK]].
- Guild messages sharded by `guild_id` in a dedicated guild-messages ES cell. Big Freaking Guilds (BGFs) get specialized multi-shard indices.
- Direct messages (DMs) sharded by `user-id` in a separate user-dm-messages ES cell for isolation.
- Zero-downtime reindexing for BFG migrations using [[parallel index writes]] during cutover.
- ES stores attachment names and message text but only returns `message_id`, `channel_id` and `guild_id` to avoid data duplication with ScyllaDB.![[Pasted image 20260730135208.png]]
### Building Blocks of Scalability
#### Erlang and Elixir
- Erlang is a programming language known for its concurrency, fault tolerance and distributed computing capabilities
- Elixir is build on top of Erlang's virtual machine ([[BEAM]]). It's a dynamic, functional programming language designed for building scalable and maintainable applications.
- Hot Code Reloading. It's ability to hot-swap code without service interruption enables Discord to deploy updates and perform maintenance tasks minimizing downtime and ensuring uninterrupted service
#### Microservices Architecture
- Microservices allow independent development, deployment and scaling of different components so that Discord can iterate quickly, scale horizontally and stay agile.
#### [[Sharding]]
- It employs sharding to distribute its user base and workload across multiple servers and databases, preventing any single component from becoming a bottleneck and enabling horizontal scaling as user traffic grows.
#### [[Horizontal Scaling]]
- Its infrastructure is designed to scale horizontally by adding more servers and resources as demand increases.
- Load balancers distribute incoming traffic across multiple servers, ensuring optimal resource usage and performance.
- Autoscaling mechanisms automatically adjust server capacity based on traffic patterns, allowing Discord to handle spikes in user activity while minimizing costs on low demand periods.
#### Async Communication
- Discord's backend communicates asynchronously using message queues, event driven architectures and [pub/sub](obsidian://open?vault=Code&file=_Study%2F_Concepts%2FPub-Sub) systems.
- It enables services to decouple from each other, improving resilience, scalability and fault tolerance.

### Concurrency and Coordination: The Actor Model
- **Actors.**
	- Each actor encapsulates its own state and behaviour. Various components of the system, such as user auth, message handling services and voice chats, can be modeled as actors.
- **Async communication.**
	- Actors communicate asynchronously by exchanging messages.
- **Concurrency.**
	- The backend can handle multiple concurrent user interactions by using actors. Each actor operates independently and can process messages concurrently without blocking other actors.
- **Fault tolerance.**
	- The actor model inherently supports fault tolerance by isolating actors from each other. If one actor encounters an error or failure, it does not affect the state or behaviour of other actors. Supervision mechanisms can monitor actors' health and restart them if necessary, ensuring system resilience.

### Optimizations for Low Latency
- **Proximity routing.** Connects users to nearby servers to reduce network latency
- **Global CDN.** Caches static content closer to users for faster delivery
- **VoIP Optimization.** Uses WebRTC and [[Opus codec]] for low latency voice communication
- **WebSocket Protocol.** Enables real-time, bidirectional communication with minimal overhead.
- **Connection resilience.** Handles network disruptions and packet loss efficiently
- **Server Load Balancing.** Distributes traffic evenly across servers to prevent overloading
- **Asynchronous Processing.** Handles requests concurrently without blocking to reduce latency.
- **Client-Side Optimization.** Implements efficient algorithms and minimizes network requests for faster interactions.
### Caching Strategies
- **Global CDN**
	- It utilizes a CDN to cache an distribute static content like images, emojis, and file attachments.
	- The content is cached on edge servers located worldwide, closer to users, reducing latency and improving content delivery speed.
- **Message and media caching**
	- Discord caches frequently accessed messages and media files to reduce repeated retrieval from backend servers.
	- Cached messages are stored locally on the client's device or on Discord servers, depending on the user preferences and message activity.
- **Metadata caching**
	- Cached metadata includes user profiles, server configurations, channel properties or message timestamps.
	- If caches metadata related to users to optimize data retrieval.
- **Presence caching**
	- Things such as online status, activity and status messages are cached to quickly display user availability to others.
	- It reduces the need for frequent status updates and queries to backend servers, improving responsiveness and UX.
- **Rate Limiting and Expiry Policies**
	- They are implemented to manage cache size, prevent cache pollution and ensure updated data.
- **Cache Invalidation**
	- Discord uses cache invalidation mechanisms to updated cached data when underlying data changes.
	- Events such as message edits, user status updates, channel and server changes trigger cache invalidation to ensure data consistency and accuracy.
### Managing Asynchronous Operations
- **Asynchronous Programming Model**
	- Backend services are designed using an asynchronous programming model, allowing multiple tasks to run concurrently without blocking each other.
	- Asynchronous operations are executed concurrently to maximize resource utilization and responsiveness, like handling user requests, processing messages and managing voice communications
- **Event-Driven Architecture**
	- Components react to events and messages asynchronously.
	- User actions, message updates and server events trigger corresponding event handlers, allowing services to respond dynamically to user interactions and system events in real time.
- **Non-Blocking I/O**
	- It enables Discord services to handle multiple concurrent connections, message exchanges and data transfers efficiently without delaying other operations.
	- It avoids blocking threads and processes while waiting for I/O operations to complete.
- **Callbacks and Promises**
	- [[Callbacks]] are functions passed as arguments to async functions, allowing them to be executed once the operation completes.
	- [[Promises]] provide a cleaner and more structured way to handle asynchronous code, enabling sequential execution of async tasks and error handling.
	- They are used to manage async operations and handle their results asynchronously.
- **Message queues and Pub/Sub Systems**
	- Message queues enable decoupled communications between services, to send and receive messages without direct dependencies.
	- Pub/sub systems allow broadcasting messages to multiple subscribers, enabling notifications and updates across Discord's infrastructure in real time.
	- Both systems manage asynchronous communication and coordination between distributed components.
### Challenges and Scaling Considerations
- **Data consistency.**
	- Maintaining data consistency across distributed systems demands more attention when managing transactions and state
- **Fault tolerance.**
	- Another challenge is designing systems and applications that are capable of failing with minimal damage to the system, to be able to run through the failure easily.
- **Resource management.**
	- It's required to be able to manage and process workloads efficiently during the time that they are used.
- **Security preserving.**
	- It's necessary to protect personal general messages, phone calls or any documents from vulnerabilities, attacks or any other form of intrusion by other individuals.


### Resources:
- https://docs.discord.com/developers/events/gateway
- https://discord.com/category/engineering
- https://bytebytego.com/guides/how-discord-stores-trillions-of-messages/
- https://hackernoon.com/inside-discords-architecture-at-scale
- https://databasesample.com/database/discord-database
- https://ggprompts.com/architecture/discord/
- https://sysdesign.wiki/systems/discord/
- https://medium.com/@sohail_saifi/the-genius-architecture-behind-discords-voice-chat-that-zoom-could-learn-from-1da9a8c5b08f
- https://www.geeksforgeeks.org/system-design/how-discord-scaled-to-15-million-users-on-one-server/
- https://javatsc.substack.com/p/full-architecture-diagram-discord
- https://javatsc.substack.com/s/building-discord-from-socket-to-scale/