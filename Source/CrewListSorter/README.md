# CrewListSorter

`CrewListSorter.dll` adds sorting controls above the stock VAB/SPH Available
Crew list. It supports stock order and case-insensitive displayed-name order,
each in ascending or descending direction.

The plugin swaps only the `UIListItem` objects already created by the stock
`CrewAssignmentDialog`. It does not reorder `KerbalRoster`, rebuild the list,
or change the vessel manifest, so eligibility decisions made by stock KSP or
CrewRandR remain intact. The stock Fill button follows the visible sorted
order.

The selected mode and direction are stored as a global UI preference through
KSP's `PluginConfiguration`. No Harmony dependency is required.

Sorting by last mission is deliberately deferred. Stock career logs do not
contain a timestamp that can be compared between different crew members.
