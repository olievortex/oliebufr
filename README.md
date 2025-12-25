# oliebufr
A C# library to decode BUFR version 3 and 4 files. It has been tested against radiosonde files. It can decode BUFR compression.

The library has 100% code coverage and sample files.

The BufrFile contains 1 or more BufrMessages. A file can contain multiple BUFR messages.

A BufrMessage contains 1 or more Subsets. This is analogous to multiple records in a spreadsheet. But this is not the only way multiple records can represented.

A Subset contains 1 or more IBufrElements. This is where the data is located. Both the data and metadata are included.

The BufrReplication IBufrElement can contain multiple records inside of it. This is the other way to encode multiple rows of data (as opposed to multiple subsets)

Implementation Note: The sequences are "flattened" into its component elements. Therefore you will not find Sequence elements in the result.

# Sample files
Included are two sample files in the src/OlieBufr.Cli/Samples folder. The file without an extension is a Version 3 file, while the one ending in .C is a Version 4 file.

# Adding new sequences / elements
This library only has the bare minimum sequences and elements to decode radiosonde files.

Add new sequences to the 'sequences.json' file inside the src/OlieBufr.Lib folder. The changes will take effect upon the next compile/build.

Add new elements to the 'elements.csv' file inside the src/OlieBufr.Lib folder. The changes will take effect upon the next compile/build.

# Radiosonde Data
[Global BUFR Data Stream](https://www.ncei.noaa.gov/access/metadata/landing-page/bin/iso?id=gov.noaa.ncdc:C01500)

# The BUFR Format
[A Primer on Writing BUFR templates (Yves Pelletier)](https://eumetnet.eu/wp-content/uploads/2025/05/OPERA_BUFR_template_primer_V1.4.pdf)

[BUFR Reference Manual (Milan Dragosavac)](https://www.ecmwf.int/sites/default/files/elibrary/2008/80926-bufr-reference-manual_0.pdf)

[BUFR/PrepBUFR User's Guide (Developmental Testbed Center)](https://dtcenter.org/sites/default/files/community-code/gsi/downloads/BUFR/BUFR_PrepBUFR_User_Guide_v1.pdf)

[BUFR edition 3 and CREX edition 1](https://community.wmo.int/site/knowledge-hub/programmes-and-initiatives/wmo-information-system-wis/bufr-edition-3-and-crex-edition-1)

# Sequence / Element lookup
[BUFR Table B (Element definitions)](https://www.nco.ncep.noaa.gov/sib/jeff/TableB_0_STDv31_LOC7.html#class00)

[BUFR Table D (Sequence definitions)](https://www.nco.ncep.noaa.gov/sib/jeff/TableD_0_STDv31_LOC7.html#category00)

[BUFR Table C (Operation definitions)](https://wmoomm.sharepoint.com/sites/wmocpdb/eve_activityarea/Forms/AllItems.aspx?id=%2Fsites%2Fwmocpdb%2Feve%5Factivityarea%2FWMO%20Codes%2FWMO306%5FvI2%2FPrevEDITIONS%2FBUFR3CREX1%2FWMO306%5FvI2%5FBUFR3%5FTableC%5Fen%2Epdf&parent=%2Fsites%2Fwmocpdb%2Feve%5Factivityarea%2FWMO%20Codes%2FWMO306%5FvI2%2FPrevEDITIONS%2FBUFR3CREX1&p=true&ga=1)

# Known Issues
Delayed replication for a compressed file is not yet implemented. Decompression of string data is not yet implemented. Many sequences, elements, and operations are not implemented.