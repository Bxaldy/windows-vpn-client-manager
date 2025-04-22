Windows VPN Client Manager
A Windows-based enterprise application for managing VPN connections. It integrates with a custom-secured version of the VPN Connection Aggregator and is designed to work in environments using two separate LDAP directories—one for domain authentication and another for VPN access control.

🧩 Architecture Overview
The application supports environments with segregated Active Directory structures, allowing:

User Authentication through a Corporate Domain LDAP.

VPN Access Management via a dedicated VPN LDAP.

This setup enhances both security and scalability by decoupling user login and VPN access.

🔐 Group-Based Login System

Login access is controlled via Active Directory group membership:

Users must be members of the vpn_app security group (in the VPN LDAP) to log in to the application.

Different interfaces are shown based on user roles:

Sysadmins (members of sysadmin group) are redirected to an administrative dashboard.

Standard users (members of vpn_app) are directed to a simplified interface showing only their allowed VPN connections.

🚀 Features

Dual LDAP integration:

Corporate AD for authenticating users

VPN AD for controlling VPN access

Group-based login control (vpn_app)

Role-based interface: sysadmin vs. end user

Integration with a hardened VPN Connection Aggregator

Real-time VPN status

WPF-based GUI

Remote Desktop

SSH | and powershell script execution

Copy IP

Ping IP

🔧 Prerequisites
Windows 10 or later

.NET Framework 4.7.2+

Access to two Active Directory environments:

Corporate LDAP (for user login)

VPN LDAP (for VPN access groups like vpn_app)

Configured VPN Connection Aggregator

⚙️ Configuration

Clone the Repository:
git clone https://github.com/Bxaldy/windows-vpn-client-manager.git
Update App.config


👤 How It Works

Users authenticate via the Corporate LDAP.

Application checks if the user is part of the vpn_app group in the VPN LDAP.

Queries data from an SQL Server Database (I used a secure version of [VPN-CONNECTION-AGGREGATOR](https://github.com/Bxaldy/vpn-connection-aggregator) that updates all the vpn clients according to my needs every few seconds - I created the aggregator for this WPF app)  


Shows VPNs in a datatable

Right Click has functions like : remote desktop, copy ip, ping ip, ssh (you can also execute powershell scripts using ssh). 


Role Routing:

If also part of sysadmin, the user is shown the sysadmin dashboard.

Otherwise, the user sees the standard VPN selection interface.

VPN Access: The app queries the VPN LDAP to determine which VPNs the user can connect to.

Connection Handling: All VPN actions are routed through the VPN Connection Aggregator.

📸 Screenshots :

![1](https://github.com/user-attachments/assets/984a2d7d-2b7a-487d-aa6a-2b4b5627e970)



🤝 Contributing

Pull requests are welcomed. 

🪪 License
Not to be used commercially or create a commercial app derivated from this project.

⚠️ Disclaimer

This project was developed in my spare time to fulfill an internal company need—specifically, to replace LogMeIn Hamachi with an in-house solution.

I am not a professional developer, so some parts of the code may not follow best practices or be highly optimized.

While the core logic works, you are responsible for reviewing and adapting the security to fit your environment.

You may encounter comments or variable names in Romanian.

There are more mature open-source alternatives available; this project was built to address specific internal requirements at the time.

I drank a lot of beer making this, if you ever want to send me a beer as compansation for this 🐂💩, this is my rev @gabe2099

  ![SpincatGIF](https://github.com/user-attachments/assets/51f863d9-7869-4ba1-88f5-ef563b3c319f)



