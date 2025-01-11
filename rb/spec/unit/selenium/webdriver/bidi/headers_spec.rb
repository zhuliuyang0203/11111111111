# frozen_string_literal: true

# Licensed to the Software Freedom Conservancy (SFC) under one
# or more contributor license agreements.  See the NOTICE file
# distributed with this work for additional information
# regarding copyright ownership.  The SFC licenses this file
# to you under the Apache License, Version 2.0 (the
# "License"); you may not use this file except in compliance
# with the License.  You may obtain a copy of the License at
#
#   http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing,
# software distributed under the License is distributed on an
# "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
# KIND, either express or implied.  See the License for the
# specific language governing permissions and limitations
# under the License.

require File.expand_path('../spec_helper', __dir__)
require File.expand_path('../../../../../lib/selenium/webdriver/bidi/network/headers', __dir__)

module Selenium
  module WebDriver
    class BiDi
      describe Headers do
        let(:headers) { described_class.new }

        describe '#initialize' do
          it 'initializes an empty headers hash' do
            expect(headers.all).to eq({})
          end
        end

        describe '#all' do
          it 'returns the underlying headers hash' do
            headers.add_header('Authorization', 'Bearer abc123')
            expect(headers.all).to eq({'Authorization' => 'Bearer abc123'})
          end
        end

        describe '#add_header' do
          it 'adds a header to the internal store' do
            headers.add_header('Content-Type', 'application/json')
            expect(headers['Content-Type']).to eq('application/json')
          end

          it 'updates an existing header if the name already exists' do
            headers.add_header('Content-Type', 'text/html')
            headers.add_header('Content-Type', 'application/json')
            expect(headers['Content-Type']).to eq('application/json')
          end
        end

        describe '#remove_header' do
          it 'removes a header by name' do
            headers.add_header('X-Custom-Header', 'foo')
            headers.remove_header('X-Custom-Header')
            expect(headers['X-Custom-Header']).to be_nil
          end

          it 'does not raise an error if header does not exist' do
            expect { headers.remove_header('Non-Existent') }.not_to raise_error
          end
        end

        describe '#[]=' do
          it 'adds or updates a header using bracket assignment' do
            headers['Cache-Control'] = 'no-cache'
            expect(headers['Cache-Control']).to eq('no-cache')

            headers['Cache-Control'] = 'private'
            expect(headers['Cache-Control']).to eq('private')
          end
        end

        describe '#[]' do
          it 'retrieves the value of a header by name' do
            headers['Host'] = 'example.com'
            expect(headers['Host']).to eq('example.com')
          end

          it 'returns nil for unknown headers' do
            expect(headers['Does-Not-Exist']).to be_nil
          end
        end

        describe '#delete' do
          it 'removes a header via bracket-based delete' do
            headers['Accept'] = 'text/html'
            headers.delete('Accept')
            expect(headers['Accept']).to be_nil
          end
        end

        describe '#serialize' do
          it 'returns an array of header hashes in the correct format' do
            headers['Accept'] = 'application/json'
            headers.add_header('User-Agent', 'MyAgent/1.0')

            serialized = headers.serialize
            expect(serialized).to be_an(Array)
            expect(serialized.size).to eq(2)

            accept_item = serialized.find { |h| h[:name] == 'Accept' }
            expect(accept_item).not_to be_nil
            expect(accept_item[:value]).to eq({type: 'string', value: 'application/json'})

            ua_item = serialized.find { |h| h[:name] == 'User-Agent' }
            expect(ua_item).not_to be_nil
            expect(ua_item[:value]).to eq({type: 'string', value: 'MyAgent/1.0'})
          end

          it 'returns an empty array if no headers are set' do
            expect(headers.serialize).to eq([])
          end
        end
      end
    end
  end
end
