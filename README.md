# Nidhogg.NET
A Yggdrasil Authentication library for C#

## Project
This project was realized using Jetbrains Rider IDE. It should be compatible with Microsoft Visual Studio though.
All files required for compiling are within this repository. This project depends on RestSharp library, a nuget link
is provided, so the dependency can be resolved.

## Usage
To use the library, just create a YggdrasilClient object. It provides a method for every Yggdrasil functionality, which
require different parameters. The parameters mainly consist of two structures: ```AccountCredentials``` and ```Session```.
Those structures must be filled with the information, Nidhogg needs. Every method is documentend with its usage and which
information must be provided.

Yggdrasil errors are converted into different exceptions. Each method is documented with every exception that is expected to
be thrown, if an error occures. Any other exception is either a connection problem or highly unexpected.
