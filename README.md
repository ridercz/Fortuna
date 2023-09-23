# Fortuna

Fortuna is a simple software to create scratch lottery tickets. It can generate any number of tickets and any number of winners. It is written in .NET and it's a multiplatform software. It can be used on Windows, Linux and Mac.

Getting the scratch surface stickers is easy -- you can [get them for cheap on AliExpress](https://s.click.aliexpress.com/e/_DlWrFLH) in various sizes and designs. The trouble is generating randomized printing data.

> This software is not intended to be used for real lottery tickets. It's just a fun project for prizes on parties or similar events. Check your local laws and regulations before organizing such thing.

## How to use

### Create a list of prizes

First step is to prepare a list of prizes in a CSV file:

```csv
# Name;Count
First price with a pretty long name;1
Second price;2
Third price;3
Sorry, bad luck.\nTry again!;10
```

* The first column is the prize name and the second column is the number of prizes of this type. 
* You can wrap long names using the `\n` sequence.
* You can use any separator, but semicolon is recommended and default. 
* The file must be encoded in UTF-8. 

For single-field tickets you have to include number of non-winning tickets with appropriate text (see above). For multi-field tickets, enter just the prizes, required number of non-winning tickets will be generated automatically.

### Prepare ticket data

Second step is to prepare ticket data. This will generate data to be printed on tickets, based on prizes available. 

There are two types of tickets:

* Single-field tickets: each ticket has only one field with a prize name.
* Multi-field tickets: each ticket has multiple fields with prize names. Several fields must be the same to win.

Both types of tickets have serial numbers. The serial number is random and can be composed from any characters. It can be printed in human-readable form and/or as a barcode. If you have a list of all tickets (created from the ticket data file), you can check which tickets are winning and which are not.

#### Prepare single-field tickets

Run the `fortuna prepare single` command:

Usage: `fortuna prepare single [options] <input-file> <output-file>`

Argument | Long name | Meaning | Default value
-------- | --------- | ------- | -------------
`<input-file>` | | Name of CSV file containing list of prizes.
`<output-file>`| | Name of JSON file to generate. | 
`-cs <string>` | `--csv-separator <string>` | Character sequence used for CSV field separator, ie `,` or `TAB` | `;`
`-cc <string>` | `--csv-comment <string>` | Character sequence used as CSV comment line indicator. | `#`
`-sl <length>` | `--serial-length <length>` | Length of randomly generated serial numbers. | `10`
`-sc <string>` | `--serial-characters <string>` | Characters permitted in randomly generated serial number. | `0123456789ABCDEFGHKLMNPSTUWXYZ`
`-sp <string>` | `--serial-number-prefix <string>` | Prefix for all serial numbers (ie. batch number).

#### Prepare multi-field tickets

Run the `fortuna prepare multi` command:

Usage: `fortuna prepare multi [options] <input-file> <output-file> <ticket-count>`

Argument | Long name | Meaning | Default value
-------- | --------- | ------- | -------------
`<input-file>` | | Name of CSV file containing list of prizes.
`<output-file>`| | Name of JSON file to generate.
`<ticket-count>` | | Number of tickets to generate.
`-fn <number>` | `--fields <number>` | Number of fields to generate. | `3`
`-fw <number>` | `--fields-to-win <number>` | Number of fields with same content required to win. | `3`
`-cs <string>` | `--csv-separator <string>` | Character sequence used for CSV field separator, ie `,` or `TAB` | `;`
`-cc <string>` | `--csv-comment <string>` | Character sequence used as CSV comment line indicator. | `#`
`-sl <length>` | `--serial-length <length>` | Length of randomly generated serial numbers. | `10`
`-sc <string>` | `--serial-characters <string>` | Characters permitted in randomly generated serial number. | `0123456789ABCDEFGHKLMNPSTUWXYZ`
`-sp <string>` | `--serial-number-prefix <string>` | Prefix for all serial numbers (ie. batch number).

### Create background images

Next, create PNG images containing the fixed background of all tickets. Size of the images has to match the resolution you want to use for print. 

For example to print A6 (quarter of A4, 105 x 148.5 mm) tickets at 300 dpi you need the following image size:

* **Width:** 105 _mm_ * 300 _dpi_ / 25.4 _mm/in_ = **1240 _px_**
* **Height:** 148.5 _mm_ * 300 _dpi_ / 25.4 _mm/in_ = **1754 _px_**

The image has to have space for the prize field(s) and optionally the serial number and serial number barcode. Examples:

* Single field: [PNG](ticket-single.png), [PSD](ticket-single.psd)
* Multiple fields: [PNG](ticket-multi.png), [PSD](ticket-multi.psd)

### Create layout files

Layout files are JSON files containing definition of the areas on image where fields, serial numbers and barcode are supposed to be generated and how they are supposed to be formatted.

This is example for a [single-field ticket](layout-single.json):

```json
{
  "BaseImage": "ticket-single.png",
  "BarcodePosition": {
    "X": 200,
    "Y": 1050,
    "Width": 473,
    "Height": 100
  },
  "SerialNumberPosition": {
    "X": 200,
    "Y": 1150,
    "Width": 473,
    "Height": 40
  },
  "SerialNumberStyle": {
    "Color": "#ffffff",
    "Font": "Consolas",
    "Size": 24,
    "Bold": true,
    "Italic": false
  },
  "FieldStyle": {
    "Color": "#000000",
    "Font": "Arial",
    "Size": 60,
    "Bold": false,
    "Italic": false
  },
  "Fields": [
    {
      "X": 224,
      "Y": 525,
      "Width": 425,
      "Height": 189
    }
  ]
}
```

* `BaseImage` is path to the background image.
* `BarcodePosition` is position and size of a barcode. The `X` and `Y` coordinates are from top left corner. Barcode is always printed in black and the layout should be prepared for it and include the quiet zone for it as well.
* `SerialNumberPosition` is position and size of a space for serial number. The serial number is centered in this space.
* `SerialNumberStyle` defines font and color for the serial number text.
* `FieldStyle`  defines font and color for the field text.
* `Fields` is a collection of field definitions, each having its size and space.

### Generate ticket images

Next, use the `fortuna generate` command to generate images from the ticket data, layout files and base image.

Usage: `fortuna generate [options] <ticket-data-file> <ticket-layout-file> <output-folder>`

Argument | Value | Default
-------- | ----- | -------
`<ticket-data-file>` | JSON file with ticket data, prepared with the `prepare` command.
`<ticket-layout-file>` | JSON file containing layout.
`<output-folder>` | Folder where generated output images are to be stored.
`--dpi <number>` | Set the DPI resolution of the generated images. | `300`

After running this command, the output folder will contain a bunch of image files, named according to random serial number. You can print these images to obtain lottery tickets.

### Merge images to sheets (optional)

If you are printing the image files on regular home or office printer, you'll probably like them to be organized in pages. So you can put together ie. four A6 tickets to form a single A4 page etd. There is the `fortuna merge` command for that.

Usage: `fortuna merge [options] <source-folder> <target-folder> <columns> <rows>`

Argument | Long name | Meaning | Default value
-------- | --------- | ------- | -------------
`<source-folder>` | | Folder containing images to merge, created by `generate` command.
`<target-folder>` | | Folder where the merged files will be stored.
`<columns>` | | How many images is to be merged horizontally.
`<rows>` | | How many images is to be merged vertically.
`-cm` | `--cut-marks` | Draw cut marks on the page.
`-cml <length>` | `--cut-mark-length <length>` | Length of the cut marks in mm. | `5`
`--dpi <number>` | | Set the DPI resolution of the generated images. | `300`