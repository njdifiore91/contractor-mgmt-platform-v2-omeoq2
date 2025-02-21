import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import axios from 'axios';
import {
  getUsers,
  getUser,
  createUser,
  updateUser,
  deleteUser,
  getCodeTypes,
  createCodeType,
  updateCodeType,
  deleteCodeType,
  getQuickLinks,
  createQuickLink,
  updateQuickLink,
  deleteQuickLink
} from '@/services/api/admin';
import { User, CodeType, QuickLink } from '@/types/admin';

// Mock axios
vi.mock('axios');

describe('Admin API Service Tests', () => {
  // Test data
  const mockUser: User = {
    id: 1,
    firstName: 'John',
    lastName: 'Doe',
    email: 'john.doe@example.com',
    emailConfirmed: true,
    roles: ['Admin'],
    isActive: true,
    createdAt: new Date(),
    lastLoginAt: new Date(),
    receiveEmails: true,
    password: 'hashedPassword'
  };

  const mockCodeType: CodeType = {
    id: 1,
    name: 'Test Code Type',
    description: 'Test Description',
    isActive: true,
    codes: [
      {
        id: 1,
        value: 'TEST',
        description: 'Test Code',
        isExpireable: true,
        isActive: true,
        expiresAt: null
      }
    ],
    createdAt: new Date(),
    createdBy: 1,
    updatedAt: null,
    updatedBy: null
  };

  const mockQuickLink: QuickLink = {
    id: 1,
    label: 'Test Link',
    link: 'https://example.com',
    order: 1,
    isActive: true,
    createdAt: new Date(),
    createdBy: 1,
    updatedAt: null,
    updatedBy: null
  };

  beforeEach(() => {
    vi.resetAllMocks();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  describe('User Management Tests', () => {
    it('should get users when user has Edit Users permission', async () => {
      const mockResponse = { data: [mockUser] };
      vi.mocked(axios.get).mockResolvedValueOnce(mockResponse);

      const result = await getUsers();
      expect(result).toEqual([mockUser]);
      expect(axios.get).toHaveBeenCalledWith('/api/admin/users', expect.any(Object));
    });

    it('should throw PermissionError when user lacks Edit Users permission', async () => {
      vi.mocked(axios.get).mockRejectedValueOnce({
        response: { status: 403 },
        isAxiosError: true
      });

      await expect(getUsers()).rejects.toThrow('User lacks permission to edit users');
    });

    it('should create user with valid data', async () => {
      const newUser = { ...mockUser };
      delete newUser.id;
      vi.mocked(axios.post).mockResolvedValueOnce({ data: mockUser });

      const result = await createUser(newUser);
      expect(result).toEqual(mockUser);
      expect(axios.post).toHaveBeenCalledWith('/api/admin/users', newUser, expect.any(Object));
    });

    it('should update user with valid data', async () => {
      const updateData = { firstName: 'Updated' };
      vi.mocked(axios.put).mockResolvedValueOnce({ data: { ...mockUser, ...updateData } });

      const result = await updateUser(1, updateData);
      expect(result.firstName).toBe('Updated');
      expect(axios.put).toHaveBeenCalledWith('/api/admin/users/1', updateData, expect.any(Object));
    });
  });

  describe('Code Type Management Tests', () => {
    it('should get code types when user has Edit Codes permission', async () => {
      const mockResponse = { data: [mockCodeType] };
      vi.mocked(axios.get).mockResolvedValueOnce(mockResponse);

      const result = await getCodeTypes();
      expect(result).toEqual([mockCodeType]);
      expect(axios.get).toHaveBeenCalledWith('/api/admin/code-types', expect.any(Object));
    });

    it('should throw PermissionError when user lacks Edit Codes permission', async () => {
      vi.mocked(axios.get).mockRejectedValueOnce({
        response: { status: 403 },
        isAxiosError: true
      });

      await expect(getCodeTypes()).rejects.toThrow('User lacks permission to view code types');
    });

    it('should create code type with valid data', async () => {
      const newCodeType = { ...mockCodeType };
      delete newCodeType.id;
      vi.mocked(axios.post).mockResolvedValueOnce({ data: mockCodeType });

      const result = await createCodeType(newCodeType);
      expect(result).toEqual(mockCodeType);
      expect(axios.post).toHaveBeenCalledWith('/api/admin/code-types', newCodeType, expect.any(Object));
    });

    it('should update code type with valid data', async () => {
      const updateData = { name: 'Updated Code Type' };
      vi.mocked(axios.put).mockResolvedValueOnce({ data: { ...mockCodeType, ...updateData } });

      const result = await updateCodeType(1, updateData);
      expect(result.name).toBe('Updated Code Type');
      expect(axios.put).toHaveBeenCalledWith('/api/admin/code-types/1', updateData, expect.any(Object));
    });
  });

  describe('Quick Link Management Tests', () => {
    it('should get quick links when user has Edit Links permission', async () => {
      const mockResponse = { data: [mockQuickLink] };
      vi.mocked(axios.get).mockResolvedValueOnce(mockResponse);

      const result = await getQuickLinks();
      expect(result).toEqual([mockQuickLink]);
      expect(axios.get).toHaveBeenCalledWith('/api/admin/quick-links', expect.any(Object));
    });

    it('should throw PermissionError when user lacks Edit Links permission', async () => {
      vi.mocked(axios.get).mockRejectedValueOnce({
        response: { status: 403 },
        isAxiosError: true
      });

      await expect(getQuickLinks()).rejects.toThrow('User lacks permission to view quick links');
    });

    it('should create quick link with valid data', async () => {
      const newQuickLink = { ...mockQuickLink };
      delete newQuickLink.id;
      vi.mocked(axios.post).mockResolvedValueOnce({ data: mockQuickLink });

      const result = await createQuickLink(newQuickLink);
      expect(result).toEqual(mockQuickLink);
      expect(axios.post).toHaveBeenCalledWith('/api/admin/quick-links', newQuickLink, expect.any(Object));
    });

    it('should validate quick link URL format on creation', async () => {
      const invalidQuickLink = { ...mockQuickLink, link: 'invalid-url' };
      delete invalidQuickLink.id;
      vi.mocked(axios.post).mockRejectedValueOnce({
        response: { status: 400, data: { message: 'Invalid URL format' } },
        isAxiosError: true
      });

      await expect(createQuickLink(invalidQuickLink)).rejects.toThrow('Invalid URL format');
    });

    it('should update quick link with valid data', async () => {
      const updateData = { label: 'Updated Link', order: 2 };
      vi.mocked(axios.put).mockResolvedValueOnce({ data: { ...mockQuickLink, ...updateData } });

      const result = await updateQuickLink(1, updateData);
      expect(result.label).toBe('Updated Link');
      expect(result.order).toBe(2);
      expect(axios.put).toHaveBeenCalledWith('/api/admin/quick-links/1', updateData, expect.any(Object));
    });
  });

  describe('Error Handling Tests', () => {
    it('should handle network errors appropriately', async () => {
      vi.mocked(axios.get).mockRejectedValueOnce(new Error('Network Error'));

      await expect(getUsers()).rejects.toThrow('Network Error');
    });

    it('should handle API errors with proper status codes', async () => {
      vi.mocked(axios.post).mockRejectedValueOnce({
        response: { status: 500, data: { message: 'Internal Server Error' } },
        isAxiosError: true
      });

      await expect(createUser(mockUser)).rejects.toThrow('Internal Server Error');
    });
  });
});