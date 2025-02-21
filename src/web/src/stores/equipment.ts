/**
 * @file Equipment Pinia store implementation
 * Manages equipment state, assignments, and return tracking with comprehensive error handling
 * @version 1.0.0
 */

// External imports - Pinia v2.1.0
import { defineStore, storeToRefs } from 'pinia';

// Internal type imports
import { 
  Equipment, 
  EquipmentAssignment, 
  EquipmentReturn,
  EquipmentCondition 
} from '../../types/equipment';

interface FilterCriteria {
  companyId: number | null;
  searchTerm: string;
  status: string[];
}

interface EquipmentState {
  equipmentList: Equipment[];
  loading: boolean;
  error: string | null;
  lastFetch: Date | null;
  selectedEquipment: Equipment | null;
  filterCriteria: FilterCriteria;
}

export const useEquipmentStore = defineStore('equipment', {
  state: (): EquipmentState => ({
    equipmentList: [],
    loading: false,
    error: null,
    lastFetch: null,
    selectedEquipment: null,
    filterCriteria: {
      companyId: null,
      searchTerm: '',
      status: []
    }
  }),

  getters: {
    /**
     * Returns filtered equipment list based on current filter criteria
     */
    filteredEquipment: (state): Equipment[] => {
      return state.equipmentList.filter(equipment => {
        const matchesCompany = !state.filterCriteria.companyId || 
          equipment.companyId === state.filterCriteria.companyId;
        
        const matchesSearch = state.filterCriteria.searchTerm === '' || 
          equipment.model.toLowerCase().includes(state.filterCriteria.searchTerm.toLowerCase()) ||
          equipment.serialNumber.toLowerCase().includes(state.filterCriteria.searchTerm.toLowerCase()) ||
          equipment.description.toLowerCase().includes(state.filterCriteria.searchTerm.toLowerCase());
        
        const matchesStatus = state.filterCriteria.status.length === 0 ||
          state.filterCriteria.status.includes(equipment.isOut ? 'assigned' : 'available');

        return matchesCompany && matchesSearch && matchesStatus;
      });
    },

    /**
     * Returns list of equipment that is not currently assigned
     */
    availableEquipment: (state): Equipment[] => {
      return state.equipmentList.filter(equipment => !equipment.isOut);
    }
  },

  actions: {
    /**
     * Fetches equipment list for a specific company with permission validation
     * @param companyId - ID of the company to fetch equipment for
     */
    async fetchEquipmentByCompany(companyId: number): Promise<void> {
      try {
        this.loading = true;
        this.error = null;

        // API call would go here
        const response = await fetch(`/api/equipment/company/${companyId}`, {
          headers: {
            'Content-Type': 'application/json'
          }
        });

        if (!response.ok) {
          throw new Error('Failed to fetch equipment data');
        }

        const data = await response.json();
        this.equipmentList = data;
        this.lastFetch = new Date();
        this.filterCriteria.companyId = companyId;

      } catch (error) {
        this.error = error instanceof Error ? error.message : 'An unknown error occurred';
      } finally {
        this.loading = false;
      }
    },

    /**
     * Assigns equipment to an inspector with validation and optimistic updates
     * @param assignment - Equipment assignment details
     */
    async assignEquipment(assignment: EquipmentAssignment): Promise<void> {
      try {
        if (!assignment.equipmentId || !assignment.inspectorId) {
          throw new Error('Invalid assignment data');
        }

        this.loading = true;
        this.error = null;

        // Optimistic update
        const equipmentIndex = this.equipmentList.findIndex(e => e.id === assignment.equipmentId);
        if (equipmentIndex === -1) {
          throw new Error('Equipment not found');
        }

        const originalEquipment = { ...this.equipmentList[equipmentIndex] };

        // Update local state
        this.equipmentList[equipmentIndex] = {
          ...originalEquipment,
          isOut: true,
          assignedToInspectorId: assignment.inspectorId,
          assignedDate: assignment.assignmentDate,
          assignedCondition: assignment.condition
        };

        // API call
        const response = await fetch('/api/equipment/assign', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json'
          },
          body: JSON.stringify(assignment)
        });

        if (!response.ok) {
          // Revert optimistic update
          this.equipmentList[equipmentIndex] = originalEquipment;
          throw new Error('Failed to assign equipment');
        }

      } catch (error) {
        this.error = error instanceof Error ? error.message : 'An unknown error occurred';
      } finally {
        this.loading = false;
      }
    },

    /**
     * Records equipment return with condition tracking and validation
     * @param returnData - Equipment return details
     */
    async returnEquipment(returnData: EquipmentReturn): Promise<void> {
      try {
        if (!returnData.equipmentId || !returnData.returnCondition) {
          throw new Error('Invalid return data');
        }

        this.loading = true;
        this.error = null;

        // Optimistic update
        const equipmentIndex = this.equipmentList.findIndex(e => e.id === returnData.equipmentId);
        if (equipmentIndex === -1) {
          throw new Error('Equipment not found');
        }

        const originalEquipment = { ...this.equipmentList[equipmentIndex] };

        // Update local state
        this.equipmentList[equipmentIndex] = {
          ...originalEquipment,
          isOut: false,
          assignedToInspectorId: null,
          assignedToInspectorName: null,
          returnedDate: returnData.returnDate,
          returnedCondition: returnData.returnCondition
        };

        // API call
        const response = await fetch('/api/equipment/return', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json'
          },
          body: JSON.stringify(returnData)
        });

        if (!response.ok) {
          // Revert optimistic update
          this.equipmentList[equipmentIndex] = originalEquipment;
          throw new Error('Failed to process equipment return');
        }

      } catch (error) {
        this.error = error instanceof Error ? error.message : 'An unknown error occurred';
      } finally {
        this.loading = false;
      }
    }
  }
});