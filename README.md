# Backend workshop in Yuxi Global

The purpose of this workshop is to explain the backend world to frontend developers.

This is the list of things we've checked so far in this workshop:

### How the Backend looks from the frontend perspective

- On-Premises servers and data centers
- What is the Cloud
	- IaaS
	- PaaS
	- SaaS
	
### Some networking
- OSI Model
- How does a request look like, when performed from a browser to a specific backend endpoint
- Network connecting devices
	- Routers
	- Switches
- IPs and Ports
	- Multiple network interfaces for a single device
	- Loopback
	- Wireless
	- Ethernet
- Common protocols:
	- HTTP/S
	- DNS
	- SMTP
	- DHCP
	- FTP
- Sniffers for each OSI layer
	- Fiddler and MitM
	- WireShark
	
### Some O.S. knowledge
- Difference between Processes and Threads
- Resource management

### Workshop 01: Let's create our own network protocol, just because we can
- Planning the protocol
- Using Sockets on TCP/IP
- Difference between Client and Server behaviour
- When does it make sense to create yet another network protocol, and when the reuse an existing one
- New linguo: no more Frontend/Backend; now it is: Inter-Process comunication
	- Within a device boundary
	- Among different devices
	- Synch vs. Asynch: when to use each
