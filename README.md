# Tractor
Tractor is a small program to allow custom test profiles to be rapidly created. Tractor then "drives" these profiles and 
controls the QA401 and QA450 application to make the measurments. In the current check-in, Tractor can make the followin
measurements: 
* Gain, 
* THD
* Noise
* Amplitude
* IMD
* Amp output impedance
* Amp efficiency measurements


# Changelog

## 1.310
* Added LRBalanceA01 test for measuring gain differential between L and R. Can do multiple acquisitions to enable potentiometer sweeps.

## 1.300
* Added MidiXL test for ATPI XL
* Added SerialSend test for ATPI XL
* Added SumGain test for testing L/R summing into one input
* Added SplitGain test for testing L/R splitting into 2 outputs
* Added GainSelect test for selecting the R or L channel

## 1.202
* Added test drag and drop functionality for easier reordering of test plans
* Added save state functionality for test enable/disable checkboxes
* Added Phase01 and Phase03 tests for phase measurements with QA401 and QA403
