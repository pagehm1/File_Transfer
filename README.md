# File_Transfer

Allows the user to back up multiple directories to a single destination. Also provides compression and statistics about the files being transferred.

## How to Use
Options:
  -c, --compress                              decrease destination directory size [default: False]
  -f, --files <files> (REQUIRED)              directories that are being transferred
  -d, --destination <destination> (REQUIRED)  directory that files are being transferred to
  -s, --stats                                 provides statistics on the files being transferred [default: False]
  --version                                   Show version information
  -?, -h, --help                              Show help and usage information

The only necessary values is at least one source directory and ONLY one destination directory (for now). The rest of the values are optional 
if the user wants the files suppressed. 

## Notes
